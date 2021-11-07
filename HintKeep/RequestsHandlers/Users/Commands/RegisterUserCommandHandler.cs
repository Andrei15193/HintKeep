using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using HintKeep.Exceptions;
using HintKeep.Requests.Users.Commands;
using HintKeep.Services;
using HintKeep.Storage;
using HintKeep.Storage.Entities;
using MediatR;
using Microsoft.Azure.Cosmos.Table;

namespace HintKeep.RequestsHandlers.Users.Commands
{
    public class RegisterUserCommandHandler : AsyncRequestHandler<RegisterUserCommand>
    {
        private readonly IEntityTables _entityTables;
        private readonly IEmailService _emailService;
        private readonly ISecurityService _securityService;

        public RegisterUserCommandHandler(IEntityTables entityTables, IEmailService emailService, ISecurityService securityService)
            => (_entityTables, _emailService, _securityService) = (entityTables, emailService, securityService);

        protected override async Task Handle(RegisterUserCommand command, CancellationToken cancellationToken)
        {
            var userId = Guid.NewGuid().ToString("N");
            var userEmailInLowercase = command.Email.ToLowerInvariant();

            var passwordSalt = _securityService.GeneratePasswordSalt();
            var passwordHash = _securityService.ComputePasswordHash(passwordSalt, command.Password);
            var activationToken = _securityService.GenerateConfirmationToken();

            try
            {
                await _entityTables.Users.ExecuteBatchAsync(
                    new TableBatchOperation
                    {
                        TableOperation.Insert(new UserEntity
                        {
                            PartitionKey = userEmailInLowercase.ToEncodedKeyProperty(),
                            RowKey = "details",
                            EntityType = "UserEntity",
                            Id = userId,
                            Email = command.Email,
                            Role = UserRole.Member,
                            Hint = command.Hint,
                            PasswordHash = passwordHash,
                            PasswordSalt = passwordSalt,
                            IsActive = false
                        }),
                        TableOperation.Insert(new UserActivationTokenEntity
                        {
                            PartitionKey = userEmailInLowercase.ToEncodedKeyProperty(),
                            RowKey = activationToken.Token.ToEncodedKeyProperty(),
                            EntityType = "UserActivationTokenEntity",
                            Expiration = DateTimeOffset.UtcNow.Add(activationToken.Expiration)
                        })
                    },
                    cancellationToken
                );

                await _emailService.SendAsync(command.Email, "Welcome to HintKeep!", @$"<!DOCTYPE html>
<html>
<head>
<meta http-equiv=""Content-Type"" content=""text/html;charset=UTF-8"">
<meta charset=""utf-8"">
<meta name=""charset"" content=""utf-8"">
<style>* {{ font-family: system-ui, -apple-system, ""Segoe UI"", Roboto, ""Helvetica Neue"", Arial, ""Noto Sans"", ""Liberation Sans"", sans-serif, ""Apple Color Emoji"", ""Segoe UI Emoji"", ""Segoe UI Symbol"", ""Noto Color Emoji""; }}</style>
</head>
<body>
<h1>Welcome to HintKeep!</h1>
<p>Thank for signing up to use HintKeep, we hope this service will improve the security of your accounts.
Please confirm your registration using the following code to start using this service.</p>
<pre style=""font-family: 'Courier New', Courier, monospace; font-size: 16pt;"">{activationToken.Token}</pre>
<p>You can activate your account at <a href=""https://www.hintkeep.net/confirm"">https://www.hintkeep.net/confirm</a></p>
</body>
</html>");
            }
            catch (StorageException storageException) when (storageException.RequestInformation.HttpStatusCode == (int)HttpStatusCode.Conflict)
            {
                throw new ConflictException(storageException);
            }
        }
    }
}