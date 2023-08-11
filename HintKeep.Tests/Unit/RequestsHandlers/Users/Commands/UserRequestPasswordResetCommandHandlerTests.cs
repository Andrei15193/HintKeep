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
    public class UserRequestPasswordResetCommandHandlerTests
    {
        private IEntityTables _entityTables;
        private ISecurityService _securityService;
        private IEmailService _emailService;
        private IRequestHandler<UserRequestPasswordResetCommand> _userRequestPasswordResetCommand;

        public UserRequestPasswordResetCommandHandlerTests()
        {
            _entityTables = new InMemoryEntityTables();
            _securityService = Substitute.For<ISecurityService>();
            _emailService = Substitute.For<IEmailService>();
            _userRequestPasswordResetCommand = new UserRequestPasswordResetCommandHandler(_entityTables, _securityService, _emailService);
            _entityTables.Users.Create();
        }

        [Fact]
        public async Task Handle_WhenUserDoesNotExist_ThrowsException()
        {
            _securityService
                .ComputeHash("#test@domain.com")
                .Returns("#email-hash");

            await Assert.ThrowsAsync<NotFoundException>(() => _userRequestPasswordResetCommand.Handle(new UserRequestPasswordResetCommand("#TEST@domain.com"), default));
        }

        [Fact]
        public async Task Handle_WhenInactiveUserExists_ThrowsException()
        {
            _entityTables.Users.Execute(TableOperation.Insert(new UserEntity
            {
                PartitionKey = "#email-hash".ToEncodedKeyProperty(),
                RowKey = "details",
                EntityType = "UserEntity",
                IsActive = false
            }));
            _securityService
                .ComputeHash("#test@domain.com")
                .Returns("#email-hash");

            await Assert.ThrowsAsync<NotFoundException>(() => _userRequestPasswordResetCommand.Handle(new UserRequestPasswordResetCommand("#TEST@domain.com"), default));
        }

        [Fact]
        public async Task Handle_WhenActiveUserExists_ThrowsException()
        {
            _entityTables.Users.Execute(TableOperation.Insert(new UserEntity
            {
                PartitionKey = "#email-hash".ToEncodedKeyProperty(),
                RowKey = "details",
                EntityType = "UserEntity",
                IsActive = true
            }));
            _securityService
                .ComputeHash("#test@domain.com")
                .Returns("#email-hash");
            _securityService
                .GenerateConfirmationToken()
                .Returns(new ConfirmationToken("#confirmation-token", TimeSpan.FromMinutes(60)));

            await _userRequestPasswordResetCommand.Handle(new UserRequestPasswordResetCommand("#TEST@domain.com"), default);

            var entities = _entityTables.Users.ExecuteQuery(new TableQuery());
            var userPasswordResetTokenEntity = Assert.Single(entities, entity => entity.Properties[nameof(HintKeepTableEntity.EntityType)].StringValue == "UserPasswordResetTokenEntity");
            Assert.Equal("#email-hash".ToEncodedKeyProperty(), userPasswordResetTokenEntity.PartitionKey);
            Assert.Equal("#confirmation-token".ToEncodedKeyProperty(), userPasswordResetTokenEntity.RowKey);
            Assert.Equal(2, userPasswordResetTokenEntity.Properties.Count);
            Assert.Equal("UserPasswordResetTokenEntity", userPasswordResetTokenEntity.Properties[nameof(HintKeepTableEntity.EntityType)].StringValue);
            var expiration = userPasswordResetTokenEntity.Properties[nameof(UserPasswordResetTokenEntity.Expiration)].DateTimeOffsetValue;
            Assert.True(DateTimeOffset.UtcNow.AddMinutes(55) < expiration);
            Assert.True(expiration < DateTimeOffset.UtcNow.AddMinutes(65));

            await _emailService
                .Received()
                .SendAsync("#TEST@domain.com", "HintKeep - Password Reset", Arg.Is<string>(body => body.Contains("#confirmation-token")));
            Assert.Single(_emailService.ReceivedCalls());
        }
    }
}