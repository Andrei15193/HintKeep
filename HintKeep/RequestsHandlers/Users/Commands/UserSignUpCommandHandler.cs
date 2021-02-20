using System.Threading;
using System.Threading.Tasks;
using HintKeep.Requests.Users.Commands;
using HintKeep.Storage;
using HintKeep.Storage.Entities;
using HintKeep.Services;
using MediatR;
using Microsoft.Azure.Cosmos.Table;
using System;
using System.Net;
using HintKeep.Exceptions;

namespace HintKeep.RequestsHandlers.Users.Commands
{
    public class UserSignUpCommandHandler : AsyncRequestHandler<UserSignUpCommand>
    {
        private readonly IEntityTables _entityTables;
        private readonly IRngService _rngService;
        private readonly ISaltService _saltService;
        private readonly IPasswordHashService _passwordHashService;
        private readonly IEmailService _emailService;

        public UserSignUpCommandHandler(IEntityTables entityTables, IRngService rngService, ISaltService saltService, IPasswordHashService passwordHashService, IEmailService emailService)
            => (_entityTables, _rngService, _saltService, _passwordHashService, _emailService) = (entityTables, rngService, saltService, passwordHashService, emailService);

        protected async override Task Handle(UserSignUpCommand command, CancellationToken cancellationToken)
        {
            var passwordSalt = _saltService.GetSalt();
            var passwordHash = _passwordHashService.GetHash(passwordSalt, command.Password);
            var confirmationToken = _rngService.Generate(12).ToLowerInvariant();

            var userId = Guid.NewGuid().ToString("N");
            var userEntity = new UserEntity
            {
                EntityType = "UserEntity",
                PartitionKey = userId.ToEncodedKeyProperty(),
                RowKey = "details".ToEncodedKeyProperty(),
                Email = command.Email
            };
            var loginEntity = new EmailLoginEntity
            {
                EntityType = "EmailLoginEntity",
                PartitionKey = command.Email.ToLowerInvariant().ToEncodedKeyProperty(),
                RowKey = nameof(LoginEntityType.EmailLogin).ToEncodedKeyProperty(),
                PasswordSalt = passwordSalt,
                PasswordHash = passwordHash,
                State = nameof(EmailLoginEntityState.PendingConfirmation),
                UserId = userId
            };
            var loginTokenEntity = new EmailLoginTokenEntity
            {
                EntityType = "EmailLoginTokenEntity",
                PartitionKey = command.Email.ToLowerInvariant().ToEncodedKeyProperty(),
                RowKey = (nameof(LoginEntityType.EmailLogin) + "-confirmationToken").ToEncodedKeyProperty(),
                Token = confirmationToken,
                Created = DateTime.UtcNow
            };
            try
            {
                await _entityTables.Users.ExecuteAsync(TableOperation.Insert(userEntity), cancellationToken);
                await _entityTables.Logins.ExecuteBatchAsync(new TableBatchOperation { TableOperation.Insert(loginEntity), TableOperation.Insert(loginTokenEntity) }, cancellationToken);

                await _emailService.SendAsync(new EmailMessage
                {
                    Title = "Welcome to HintKeep!",
                    To = command.Email,
                    Content = $"Welcome to HintKeep, use the following token to confirm your account. Confirmation token {confirmationToken}."
                });
            }
            catch (StorageException storageException) when (storageException.RequestInformation.HttpStatusCode == (int)HttpStatusCode.Conflict)
            {
                throw new ConflictException(storageException);
            }
        }
    }
}