using System;
using System.Threading.Tasks;
using HintKeep.Exceptions;
using HintKeep.RequestsHandlers.Users.Commands;
using HintKeep.Requests.Users.Commands;
using HintKeep.Services;
using HintKeep.Storage;
using HintKeep.Storage.Entities;
using HintKeep.Tests.Stubs;
using MediatR;
using Microsoft.Azure.Cosmos.Table;
using Moq;
using Xunit;

namespace HintKeep.Tests.Unit.RequestsHandlers.Users.Commands
{
    public class UserSignUpCommandHandlerTests : IDisposable
    {
        private readonly IEntityTables _entityTables;
        private readonly Mock<IRngService> _rngService;
        private readonly Mock<ISaltService> _saltService;
        private readonly Mock<ICryptographicHashService> _cryptographicHashService;
        private readonly Mock<IEmailService> _emailService;
        private readonly IRequestHandler<UserSignUpCommand> _userSignUpCommandHandler;

        public UserSignUpCommandHandlerTests()
        {
            _entityTables = new InMemoryEntityTables();
            _entityTables.Logins.Create();
            _entityTables.Users.Create();
            _rngService = new Mock<IRngService>();
            _saltService = new Mock<ISaltService>();
            _cryptographicHashService = new Mock<ICryptographicHashService>();
            _emailService = new Mock<IEmailService>();
            _userSignUpCommandHandler = new UserSignUpCommandHandler(_entityTables, _rngService.Object, _saltService.Object, _cryptographicHashService.Object, _emailService.Object);
        }

        public void Dispose()
        {
            _rngService.Verify();
            _rngService.VerifyNoOtherCalls();

            _saltService.Verify();
            _saltService.VerifyNoOtherCalls();

            _cryptographicHashService.Verify();
            _cryptographicHashService.VerifyNoOtherCalls();

            _emailService.Verify();
            _emailService.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task Handle_NewUser_InsertsBothUserAndConfrimationToken()
        {
            _rngService
                .Setup(rngService => rngService.Generate(12))
                .Returns("test-confirmation-token")
                .Verifiable();
            _saltService
                .Setup(saltService => saltService.GetSalt())
                .Returns("test-salt")
                .Verifiable();
            _cryptographicHashService
                .Setup(cryptographicHashService => cryptographicHashService.GetHash("test-salttest-password"))
                .Returns("test-hash")
                .Verifiable();
            _emailService
                .Setup(emailService => emailService.SendAsync(It.Is<EmailMessage>(emailMessage => emailMessage.Title == "Welcome to HintKeep!" && emailMessage.To == "test-eMail" && emailMessage.Content.Contains("test-confirmation-token"))))
                .Returns(Task.CompletedTask)
                .Verifiable();

            var command = new UserSignUpCommand
            {
                Email = "test-eMail",
                Password = "test-password"
            };
            await _userSignUpCommandHandler.Handle(command, default);

            var entities = _entityTables.Logins.ExecuteQuery(new TableQuery());
            var loginEntity = Assert.Single(entities, entity => entity.RowKey == "EmailLogin");
            Assert.Equal("test-email", loginEntity.PartitionKey);
            Assert.Equal("EmailLogin", loginEntity.RowKey);
            Assert.Equal("test-salt", loginEntity.Properties[nameof(EmailLoginEntity.PasswordSalt)].StringValue);
            Assert.Equal("test-hash", loginEntity.Properties[nameof(EmailLoginEntity.PasswordHash)].StringValue);
            Assert.Equal("PendingConfirmation", loginEntity.Properties[nameof(EmailLoginEntity.State)].StringValue);

            var loginConfirmationTokenEntity = Assert.Single(entities, entity => entity.RowKey != "EmailLogin");
            Assert.Equal("test-email", loginConfirmationTokenEntity.PartitionKey);
            Assert.Equal("EmailLogin-confirmationToken", loginConfirmationTokenEntity.RowKey);
            Assert.Equal("test-confirmation-token", loginConfirmationTokenEntity.Properties[nameof(EmailLoginTokenEntity.Token)].StringValue);
            Assert.True(DateTime.UtcNow.AddMinutes(-1) <= loginConfirmationTokenEntity.Properties[nameof(EmailLoginTokenEntity.Created)].DateTime && loginConfirmationTokenEntity.Properties[nameof(EmailLoginTokenEntity.Created)].DateTime <= DateTime.UtcNow.AddMinutes(1));

            var userEntity = Assert.Single(_entityTables.Users.ExecuteQuery(new TableQuery()));
            Assert.Equal(loginEntity.Properties[nameof(EmailLoginEntity.UserId)].StringValue, userEntity.PartitionKey);
            Assert.Equal("details", userEntity.RowKey);
            Assert.Equal("test-eMail", userEntity.Properties[nameof(UserEntity.Email)].StringValue);
        }

        [Fact]
        public async Task Handle_DuplicateUser_ThrowsEception()
        {
            _rngService
                .Setup(rngService => rngService.Generate(12))
                .Returns("test-confirmation-token")
                .Verifiable();
            _saltService
                .Setup(saltService => saltService.GetSalt())
                .Returns("test-salt")
                .Verifiable();
            _cryptographicHashService
                .Setup(cryptographicHashService => cryptographicHashService.GetHash("test-salttest-password"))
                .Returns("test-hash")
                .Verifiable();
            _entityTables.Logins.Execute(TableOperation.Insert(new TableEntity("test-email", "EmailLogin")));

            var command = new UserSignUpCommand
            {
                Email = "test-eMail",
                Password = "test-password"
            };

            var exception = await Assert.ThrowsAsync<ConflictException>(() => _userSignUpCommandHandler.Handle(command, default));
            Assert.Empty(exception.Message);
        }
    }
}