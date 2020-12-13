using System.Threading;
using System.Threading.Tasks;
using HintKeep.Commands.Users;
using HintKeep.Data;
using HintKeep.Data.Entities;
using HintKeep.Services;
using MediatR;
using Microsoft.Azure.Cosmos.Table;

namespace HintKeep.Handlers.Commands.Users
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

            var userEntity = new UserEntity
            {
                PartitionKey = command.Email.ToLowerInvariant(),
                RowKey = "user",
                Email = command.Email,
                PasswordSalt = passwordSalt,
                PasswordHash = passwordHash,
                State = (int)UserState.PendingConfirmation
            };
            var confirmationTokenEntity = new TokenEntity
            {
                PartitionKey = userEntity.PartitionKey,
                RowKey = "confirmation_tokens-" + confirmationToken,
                Token = confirmationToken,
                Intent = (int)TokenIntent.ConfirmUserRegistration
            };

            await _emailService.SendAsync(new EmailMessage
            {
                Title = "Welcome to HintKeep!",
                To = command.Email,
                Content = $"Welcome to HintKeep, use the following token to confirm your account. Confirmation token {confirmationToken}."
            });
            await _entityTables.Users.ExecuteBatchAsync(new TableBatchOperation { TableOperation.Insert(userEntity), TableOperation.Insert(confirmationTokenEntity) });
        }
    }
}