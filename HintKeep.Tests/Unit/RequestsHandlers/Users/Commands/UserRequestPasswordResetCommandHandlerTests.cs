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
    public class UserRequestPasswordResetCommandHandlerTests
    {
        private IEntityTables _entityTables;
        private Mock<ISecurityService> _securityService;
        private Mock<IEmailService> _emailService;
        private IRequestHandler<UserRequestPasswordResetCommand> _userRequestPasswordResetCommand;

        public UserRequestPasswordResetCommandHandlerTests()
        {
            _entityTables = new InMemoryEntityTables();
            _securityService = new Mock<ISecurityService>();
            _emailService = new Mock<IEmailService>();
            _userRequestPasswordResetCommand = new UserRequestPasswordResetCommandHandler(_entityTables, _securityService.Object, _emailService.Object);
            _entityTables.Users.Create();
        }

        [Fact]
        public async Task Handle_WhenUserDoesNotExist_ThrowsException()
        {
            _securityService
                .Setup(securityService => securityService.ComputeHash("#test@domain.com"))
                .Returns("#email-hash");

            await Assert.ThrowsAsync<NotFoundException>(() => _userRequestPasswordResetCommand.Handle(new UserRequestPasswordResetCommand { Email = "#TEST@domain.com" }, default));
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
                .Setup(securityService => securityService.ComputeHash("#test@domain.com"))
                .Returns("#email-hash");

            await Assert.ThrowsAsync<NotFoundException>(() => _userRequestPasswordResetCommand.Handle(new UserRequestPasswordResetCommand { Email = "#TEST@domain.com" }, default));
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
                .Setup(securityService => securityService.ComputeHash("#test@domain.com"))
                .Returns("#email-hash");
            _securityService
                .Setup(securityService => securityService.GenerateConfirmationToken())
                .Returns(new ConfirmationToken("#confirmation-token", TimeSpan.FromMinutes(60)));

            await _userRequestPasswordResetCommand.Handle(new UserRequestPasswordResetCommand { Email = "#TEST@domain.com" }, default);

            var entities = _entityTables.Users.ExecuteQuery(new TableQuery());
            var userPasswordResetTokenEntity = Assert.Single(entities, entity => entity.Properties[nameof(HintKeepTableEntity.EntityType)].StringValue == "UserPasswordResetTokenEntity");
            Assert.Equal("#email-hash".ToEncodedKeyProperty(), userPasswordResetTokenEntity.PartitionKey);
            Assert.Equal("#confirmation-token".ToEncodedKeyProperty(), userPasswordResetTokenEntity.RowKey);
            Assert.Equal(2, userPasswordResetTokenEntity.Properties.Count);
            Assert.Equal("UserPasswordResetTokenEntity", userPasswordResetTokenEntity.Properties[nameof(HintKeepTableEntity.EntityType)].StringValue);
            var expiration = userPasswordResetTokenEntity.Properties[nameof(UserPasswordResetTokenEntity.Expiration)].DateTimeOffsetValue;
            Assert.True(DateTimeOffset.UtcNow.AddMinutes(55) < expiration);
            Assert.True(expiration < DateTimeOffset.UtcNow.AddMinutes(65));

            _emailService.Verify(emailService => emailService.SendAsync("#TEST@domain.com", "HintKeep - Password Reset", It.IsRegex("#confirmation-token")), Times.Once);
            _emailService.VerifyNoOtherCalls();
        }
    }
}