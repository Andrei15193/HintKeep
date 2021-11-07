using System;
using System.Collections.Generic;
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
    public class UserRequestPasswordResetCommandHandler : AsyncRequestHandler<UserRequestPasswordResetCommand>
    {
        private readonly IEntityTables _entityTables;
        private readonly ISecurityService _securityService;
        private readonly IEmailService _emailService;

        public UserRequestPasswordResetCommandHandler(IEntityTables entityTables, ISecurityService securityService, IEmailService emailService)
            => (_entityTables, _securityService, _emailService) = (entityTables, securityService, emailService);

        protected override async Task Handle(UserRequestPasswordResetCommand command, CancellationToken cancellationToken)
        {
            var emailInLowercase = command.Email.ToLowerInvariant();
            var userEntity = (UserEntity)(await _entityTables.Users.ExecuteAsync(
                TableOperation.Retrieve<UserEntity>(emailInLowercase.ToEncodedKeyProperty(), "details", new List<string> { nameof(UserEntity.IsActive) }),
                cancellationToken
            )).Result;
            if (userEntity is null || !userEntity.IsActive)
                throw new NotFoundException();

            var confirmationToken = _securityService.GenerateConfirmationToken();
            await _entityTables.Users.ExecuteAsync(
                TableOperation.Insert(new UserPasswordResetTokenEntity
                {
                    PartitionKey = emailInLowercase.ToEncodedKeyProperty(),
                    RowKey = confirmationToken.Token.ToEncodedKeyProperty(),
                    EntityType = "UserPasswordResetTokenEntity",
                    Expiration = DateTimeOffset.UtcNow.Add(confirmationToken.Expiration)
                }),
                cancellationToken
            );

            await _emailService.SendAsync(
                command.Email,
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
<p>You have requested a password reset, please use the token below to reset your password at <a href=""https://www.hintkeep.net/password-reset"">https://www.hintkeep.net/password-reset</a>.</p>
<pre style=""font-family: 'Courier New', Courier, monospace; font-size: 16pt;"">{confirmationToken.Token}</pre>
<p>Thank you for using our services!</p>
</body>
</html>"
            );
        }
    }
}