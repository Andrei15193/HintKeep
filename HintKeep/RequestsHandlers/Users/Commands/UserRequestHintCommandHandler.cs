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

namespace HintKeep.RequestsHandlers.Users.Commands{
    public class UserRequestHintCommandHandler : AsyncRequestHandler<UserRequestHintCommand>
    {
        private readonly IEntityTables _entityTables;
        private readonly IEmailService _emailService;

        public UserRequestHintCommandHandler(IEntityTables entityTables, IEmailService emailService)
            => (_entityTables, _emailService) = (entityTables, emailService);

        protected override async Task Handle(UserRequestHintCommand command, CancellationToken cancellationToken)
        {
            var emailInLowercase = command.Email.ToLowerInvariant();
            var userEntity = (UserEntity)(await _entityTables.Users.ExecuteAsync(
                TableOperation.Retrieve<UserEntity>(emailInLowercase.ToEncodedKeyProperty(), "details", new List<string> { nameof(UserEntity.IsActive), nameof(UserEntity.Hint) }),
                cancellationToken
            )).Result;
            if (userEntity is null || !userEntity.IsActive)
                throw new NotFoundException();

            await _emailService.SendAsync(
                command.Email,
                "HintKeep - Account Hint",
                $@"<!DOCTYPE html>
<html>
<head>
<meta http-equiv=""Content-Type"" content=""text/html;charset=UTF-8"">
<meta charset=""utf-8"">
<meta name=""charset"" content=""utf-8"">
<style>* {{ font-family: system-ui, -apple-system, ""Segoe UI"", Roboto, ""Helvetica Neue"", Arial, ""Noto Sans"", ""Liberation Sans"", sans-serif, ""Apple Color Emoji"", ""Segoe UI Emoji"", ""Segoe UI Symbol"", ""Noto Color Emoji""; }}</style>
</head>
<body>
<h1>Account Hint</h1>
<p>Greetings,</p>
<p>You have requested your account hint for recovering your account. Here it is: {userEntity.Hint}.</p>
<p>Thank you for using our services!</p>
</body>
</html>"
            );
        }
    }
}