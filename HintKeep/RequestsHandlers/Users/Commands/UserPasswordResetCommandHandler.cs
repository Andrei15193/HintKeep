using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HintKeep.Exceptions;
using HintKeep.Services;
using HintKeep.Storage;
using HintKeep.Storage.Entities;
using MediatR;
using Microsoft.Azure.Cosmos.Table;

namespace HintKeep.Requests.Users.Commands
{
    public class UserPasswordResetCommandHandler : AsyncRequestHandler<UserPasswordResetCommand>
    {
        private readonly IEntityTables _entityTables;
        private readonly ISecurityService _securityService;
        private readonly IEmailService _emailService;

        public UserPasswordResetCommandHandler(IEntityTables entityTables, ISecurityService securityService, IEmailService emailService)
            => (_entityTables, _securityService, _emailService) = (entityTables, securityService, emailService);

        protected override async Task Handle(UserPasswordResetCommand command, CancellationToken cancellationToken)
        {
            var userPasswordResetTokenEntity = (
                await _entityTables.Users.ExecuteQuerySegmentedAsync(
                    new TableQuery<UserPasswordResetTokenEntity>()
                        .Where(
                            TableQuery.CombineFilters(
                                TableQuery.GenerateFilterCondition(nameof(HintKeepTableEntity.EntityType), QueryComparisons.Equal, "UserPasswordResetTokenEntity"),
                                TableOperators.And,
                                TableQuery.GenerateFilterCondition(nameof(TableEntity.RowKey), QueryComparisons.Equal, command.Token.ToEncodedKeyProperty())
                            )
                        )
                        .Take(1),
                    null,
                    cancellationToken
                )
            ).Results.SingleOrDefault();

            if (userPasswordResetTokenEntity is null)
                throw new NotFoundException();
            if (userPasswordResetTokenEntity.Expiration < DateTimeOffset.UtcNow)
            {
                await _entityTables.Users.ExecuteAsync(
                    TableOperation.Delete(userPasswordResetTokenEntity),
                    cancellationToken
                );
                throw new NotFoundException();
            }

            var userEntity = (UserEntity)(await _entityTables.Users.ExecuteAsync(TableOperation.Retrieve<UserEntity>(userPasswordResetTokenEntity.PartitionKey, "details"), cancellationToken)).Result;
            if (userEntity is null || !userEntity.IsActive)
            {
                await _entityTables.Users.ExecuteAsync(
                    TableOperation.Delete(userPasswordResetTokenEntity),
                    cancellationToken
                );
                throw new NotFoundException();
            }

            var passwordSalt = _securityService.GeneratePasswordSalt();
            var passwordHash = _securityService.ComputePasswordHash(passwordSalt, command.Password);

            await _entityTables.Users.ExecuteBatchAsync(
                new TableBatchOperation
                {
                    TableOperation.Merge(
                        new DynamicTableEntity
                        {
                            PartitionKey = userEntity.PartitionKey,
                            RowKey = userEntity.RowKey,
                            ETag = userEntity.ETag,
                            Properties = new Dictionary<string, EntityProperty>(StringComparer.Ordinal)
                            {
                                { nameof(UserEntity.PasswordSalt), EntityProperty.GeneratePropertyForString(passwordSalt) },
                                { nameof(UserEntity.PasswordHash), EntityProperty.GeneratePropertyForString(passwordHash) }
                            }
                        }
                    ),
                    TableOperation.Delete(userPasswordResetTokenEntity)
                },
                cancellationToken
            );

            await _emailService.SendAsync(
                userEntity.Email,
                "HintKeep - Password Reset",
                $@"<!DOCTYPE html>
<html>
<head>
<meta http-equiv=""Content-Type"" content=""text/html;charset=UTF-8"">
<meta charset=""utf-8"">
<meta name=""charset"" content=""utf-8"">
<style>* {{ font-family: system-ui, -apple-system, ""Segoe UI"", Roboto, ""Helvetica Neue"", Arial, ""Noto Sans"", ""Liberation Sans"", sans-serif, ""Apple Color Emoji"", ""Segoe UI Emoji"", ""Segoe UI Symbol"", ""Noto Color Emoji""; }}</style>
</head>
<body>
<h1>Password Reset</h1>
<p>Greetings,</p>
<p>Your password has been reset. If this was not you then please contact us as soon as possible at <a href=""mailto:hintkeep@gmail.com"">hintkeep@gmail.com</a> to retrieve your account!</p>
<p>Thank you for using our services!</p>
</body>
</html>"
            );
        }
    }
}