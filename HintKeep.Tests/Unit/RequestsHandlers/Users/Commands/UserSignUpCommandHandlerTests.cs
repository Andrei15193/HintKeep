using System;
using System.Threading.Tasks;
using HintKeep.Exceptions;
using HintKeep.RequestHandlers.Users.Commands;
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

            var entities = _entityTables.Users.ExecuteQuery(new TableQuery());
            var userEntity = Assert.Single(entities, entity => entity.RowKey == "user");
            var confirmationTokenEntity = Assert.Single(entities, entity => entity.RowKey != "user");
            Assert.Equal("test-email", userEntity.PartitionKey);
            Assert.Equal("user", userEntity.RowKey);
            Assert.Equal("test-eMail", userEntity.Properties[nameof(UserEntity.Email)].StringValue);
            Assert.Equal("test-salt", userEntity.Properties[nameof(UserEntity.PasswordSalt)].StringValue);
            Assert.Equal("test-hash", userEntity.Properties[nameof(UserEntity.PasswordHash)].StringValue);
            Assert.Equal(UserState.PendingConfirmation, (UserState)userEntity.Properties[nameof(UserEntity.State)].Int32Value);

            Assert.Equal("test-email", confirmationTokenEntity.PartitionKey);
            Assert.Equal("confirmation_tokens-test-confirmation-token", confirmationTokenEntity.RowKey);
            Assert.Equal("test-confirmation-token", confirmationTokenEntity.Properties[nameof(TokenEntity.Token)].StringValue);
            Assert.True(DateTime.UtcNow.AddMinutes(-1) <= confirmationTokenEntity.Properties[nameof(TokenEntity.Created)].DateTime && confirmationTokenEntity.Properties[nameof(TokenEntity.Created)].DateTime <= DateTime.UtcNow.AddMinutes(1));
            Assert.Equal(TokenIntent.ConfirmUserRegistration, (TokenIntent)confirmationTokenEntity.Properties[nameof(TokenEntity.Intent)].Int32Value);
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
            _entityTables.Users.Execute(TableOperation.Insert(new TableEntity("test-email", "user")));

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