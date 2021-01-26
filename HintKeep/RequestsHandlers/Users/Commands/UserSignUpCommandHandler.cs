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
        private readonly ICryptographicHashService _cryptographicHashService;
        private readonly IEmailService _emailService;

        public UserSignUpCommandHandler(IEntityTables entityTables, IRngService rngService, ISaltService saltService, ICryptographicHashService cryptographicHashService, IEmailService emailService)
            => (_entityTables, _rngService, _saltService, _cryptographicHashService, _emailService) = (entityTables, rngService, saltService, cryptographicHashService, emailService);

        protected async override Task Handle(UserSignUpCommand command, CancellationToken cancellationToken)
        {
            var passwordSalt = _saltService.GetSalt();
            var passwordHash = _cryptographicHashService.GetHash(passwordSalt + command.Password);
            var confirmationToken = _rngService.Generate(12).ToLowerInvariant();

            var userId = Guid.NewGuid();
            var userEntity = new UserEntity
            {
                PartitionKey = userId.ToString("D"),
                RowKey = "details",
                Email = command.Email
            };
            var loginEntity = new EmailLoginEntity
            {
                PartitionKey = command.Email.ToLowerInvariant(),
                RowKey = nameof(LoginEntityType.EmailLogin),
                PasswordSalt = passwordSalt,
                PasswordHash = passwordHash,
                State = nameof(EmailLoginEntityState.PendingConfirmation),
                UserId = userId
            };
            var loginTokenEntity = new EmailLoginTokenEntity
            {
                PartitionKey = command.Email.ToLowerInvariant(),
                RowKey = nameof(LoginEntityType.EmailLogin) + "-confirmationToken",
                Token = confirmationToken,
                Created = DateTime.UtcNow
            };
            try
            {
                await _entityTables.Users.ExecuteAsync(TableOperation.Insert(userEntity));
                await _entityTables.Logins.ExecuteBatchAsync(new TableBatchOperation { TableOperation.Insert(loginEntity), TableOperation.Insert(loginTokenEntity) });

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