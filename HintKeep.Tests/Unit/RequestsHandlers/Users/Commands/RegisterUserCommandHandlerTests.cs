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
using NSubstitute;
using Xunit;

namespace HintKeep.Tests.Unit.RequestsHandlers.Users.Commands
{
    public class RegisterUserCommandHandlerTests
    {
        private readonly IEntityTables _entityTables;
        private readonly IEmailService _emailService;
        private readonly ISecurityService _securityService;
        private readonly IRequestHandler<RegisterUserCommand> _registerUserCommandHandler;

        public RegisterUserCommandHandlerTests()
        {
            _entityTables = new InMemoryEntityTables();
            _emailService = Substitute.For<IEmailService>();
            _securityService = Substitute.For<ISecurityService>();
            _entityTables.Users.Create();
            _registerUserCommandHandler = new RegisterUserCommandHandler(_entityTables, _emailService, _securityService);
        }

        [Fact]
        public async Task Handle_WhenUserDoesNotExist_CreatesInactiveUserAndActivationToken()
        {
            _securityService
                .ComputeHash("#test@domain.com")
                .Returns("#email-hash");
            _securityService
                .GeneratePasswordSalt()
                .Returns("#password-salt");
            _securityService
                .ComputePasswordHash("#password-salt", "#test-password")
                .Returns("#password-hash");
            _securityService
                .GenerateConfirmationToken()
                .Returns(new ConfirmationToken("#activation-token", TimeSpan.FromMinutes(60)));

            await _registerUserCommandHandler.Handle(
                new RegisterUserCommand(
                    "#TEST@domain.com",
                    "#test-hint",
                    "#test-password"
                ),
                default
            );

            var entities = _entityTables.Users.ExecuteQuery(new TableQuery());
            var userEntity = Assert.Single(entities, entity => entity.Properties[nameof(HintKeepTableEntity.EntityType)].StringValue == "UserEntity");
            Assert.Equal("#email-hash".ToEncodedKeyProperty(), userEntity.PartitionKey);
            Assert.Equal("details", userEntity.RowKey);
            Assert.Equal(7, userEntity.Properties.Count);
            Assert.Equal("UserEntity", userEntity.Properties[nameof(HintKeepTableEntity.EntityType)].StringValue);
            Assert.NotEmpty(userEntity.Properties[nameof(UserEntity.Id)].StringValue);
            Assert.Equal("member", userEntity.Properties[nameof(UserEntity.Role)].StringValue);
            Assert.Equal("#test-hint", userEntity.Properties[nameof(UserEntity.Hint)].StringValue);
            Assert.Equal("#password-salt", userEntity.Properties[nameof(UserEntity.PasswordSalt)].StringValue);
            Assert.Equal("#password-hash", userEntity.Properties[nameof(UserEntity.PasswordHash)].StringValue);
            Assert.False(userEntity.Properties[nameof(UserEntity.IsActive)].BooleanValue);

            var userActivationTokenEntity = Assert.Single(entities, entity => entity.Properties[nameof(HintKeepTableEntity.EntityType)].StringValue == "UserActivationTokenEntity");
            Assert.Equal("#email-hash".ToEncodedKeyProperty(), userActivationTokenEntity.PartitionKey);
            Assert.Equal("#activation-token".ToEncodedKeyProperty(), userActivationTokenEntity.RowKey);
            Assert.Equal(2, userActivationTokenEntity.Properties.Count);
            Assert.Equal("UserActivationTokenEntity", userActivationTokenEntity.Properties[nameof(HintKeepTableEntity.EntityType)].StringValue);
            var expiration = userActivationTokenEntity.Properties[nameof(UserActivationTokenEntity.Expiration)].DateTimeOffsetValue;
            Assert.True(DateTimeOffset.UtcNow.AddMinutes(55) < expiration);
            Assert.True(expiration < DateTimeOffset.UtcNow.AddMinutes(65));

            await _emailService
                .Received()
                .SendAsync("#TEST@domain.com", "Welcome to HintKeep!", Arg.Is<string>(body => body.Contains("#activation-token")));
            Assert.Single(_emailService.ReceivedCalls());
        }

        [Fact]
        public async Task Handle_WhenUserEmailAlreadyExists_ThrowsException()
        {
            _securityService
                .ComputeHash("#test@domain.com")
                .Returns("#email-hash");
            _securityService
                .GenerateConfirmationToken()
                .Returns(new ConfirmationToken("#token", TimeSpan.Zero));
            _entityTables.Users.Execute(TableOperation.Insert(new TableEntity { PartitionKey = "#email-hash".ToEncodedKeyProperty(), RowKey = "details" }));

            await Assert.ThrowsAsync<ConflictException>(() => _registerUserCommandHandler.Handle(
                new RegisterUserCommand(
                    "#TEST@domain.com",
                    "#test-hint",
                    "#test-password"
                ),
                default
            ));

            Assert.Empty(_emailService.ReceivedCalls());
        }
    }
}