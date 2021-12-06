using System;
using System.Threading.Tasks;
using HintKeep.Exceptions;
using HintKeep.Requests.Users.Commands;
using HintKeep.RequestsHandlers.Users.Commands;
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
    public class RegisterUserCommandHandlerTests
    {
        private readonly IEntityTables _entityTables;
        private readonly Mock<IEmailService> _emailService;
        private readonly Mock<ISecurityService> _securityService;
        private readonly IRequestHandler<RegisterUserCommand> _registerUserCommandHandler;

        public RegisterUserCommandHandlerTests()
        {
            _entityTables = new InMemoryEntityTables();
            _emailService = new Mock<IEmailService>();
            _securityService = new Mock<ISecurityService>();
            _entityTables.Users.Create();
            _registerUserCommandHandler = new RegisterUserCommandHandler(_entityTables, _emailService.Object, _securityService.Object);
        }

        [Fact]
        public async Task Handle_WhenUserDoesNotExist_CreatesInactiveUserAndActivationToken()
        {
            _securityService
                .Setup(securityService => securityService.GeneratePasswordSalt())
                .Returns("#password-salt");
            _securityService
                .Setup(securityService => securityService.ComputePasswordHash("#password-salt", "#test-password"))
                .Returns("#password-hash");
            _securityService
                .Setup(securityService => securityService.GenerateConfirmationToken())
                .Returns(new ConfirmationToken("#activation-token", TimeSpan.FromMinutes(60)));

            await _registerUserCommandHandler.Handle(
                new RegisterUserCommand
                {
                    Email = "#TEST@domain.com",
                    Hint = "#test-hint",
                    Password = "#test-password"
                },
                default
            );

            var entities = _entityTables.Users.ExecuteQuery(new TableQuery());
            var userEntity = Assert.Single(entities, entity => entity.Properties[nameof(HintKeepTableEntity.EntityType)].StringValue == "UserEntity");
            Assert.Equal("#test@domain.com".ToEncodedKeyProperty(), userEntity.PartitionKey);
            Assert.Equal("details", userEntity.RowKey);
            Assert.Equal(8, userEntity.Properties.Count);
            Assert.Equal("UserEntity", userEntity.Properties[nameof(HintKeepTableEntity.EntityType)].StringValue);
            Assert.NotEmpty(userEntity.Properties[nameof(UserEntity.Id)].StringValue);
            Assert.Equal("#TEST@domain.com", userEntity.Properties[nameof(UserEntity.Email)].StringValue);
            Assert.Equal("member", userEntity.Properties[nameof(UserEntity.Role)].StringValue);
            Assert.Equal("#test-hint", userEntity.Properties[nameof(UserEntity.Hint)].StringValue);
            Assert.Equal("#password-salt", userEntity.Properties[nameof(UserEntity.PasswordSalt)].StringValue);
            Assert.Equal("#password-hash", userEntity.Properties[nameof(UserEntity.PasswordHash)].StringValue);
            Assert.False(userEntity.Properties[nameof(UserEntity.IsActive)].BooleanValue);

            var userActivationTokenEntity = Assert.Single(entities, entity => entity.Properties[nameof(HintKeepTableEntity.EntityType)].StringValue == "UserActivationTokenEntity");
            Assert.Equal("#test@domain.com".ToEncodedKeyProperty(), userActivationTokenEntity.PartitionKey);
            Assert.Equal("#activation-token".ToEncodedKeyProperty(), userActivationTokenEntity.RowKey);
            Assert.Equal(2, userActivationTokenEntity.Properties.Count);
            Assert.Equal("UserActivationTokenEntity", userActivationTokenEntity.Properties[nameof(HintKeepTableEntity.EntityType)].StringValue);
            var expiration = userActivationTokenEntity.Properties[nameof(UserActivationTokenEntity.Expiration)].DateTimeOffsetValue;
            Assert.True(DateTimeOffset.UtcNow.AddMinutes(55) < expiration);
            Assert.True(expiration < DateTimeOffset.UtcNow.AddMinutes(65));

            _emailService.Verify(emailService => emailService.SendAsync("#TEST@domain.com", "Welcome to HintKeep!", It.IsRegex("#activation-token")), Times.Once);
            _emailService.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task Handle_WhenUserEmailAlreadyExists_ThrowsException()
        {
            _securityService
                .Setup(securityService => securityService.GenerateConfirmationToken())
                .Returns(new ConfirmationToken("#token", TimeSpan.Zero));
            _entityTables.Users.Execute(TableOperation.Insert(new TableEntity { PartitionKey = "test@domain.com".ToEncodedKeyProperty(), RowKey = "details" }));

            await Assert.ThrowsAsync<ConflictException>(() => _registerUserCommandHandler.Handle(
                new RegisterUserCommand
                {
                    Email = "TEST@domain.com",
                    Hint = "test-hint",
                    Password = "test-password"
                },
                default
            ));

            _emailService.VerifyNoOtherCalls();
        }
    }
}