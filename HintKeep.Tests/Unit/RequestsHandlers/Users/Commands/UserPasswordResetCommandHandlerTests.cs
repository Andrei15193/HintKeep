using System;
using System.Threading.Tasks;
using HintKeep.Exceptions;
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
    public class UserPasswordResetCommandHandlerTests
    {
        private readonly InMemoryEntityTables _entityTables;
        private readonly Mock<ISecurityService> _securityService;
        private readonly Mock<IEmailService> _emailService;
        private readonly IRequestHandler<UserPasswordResetCommand> _userPasswordResetCommandHandler;

        public UserPasswordResetCommandHandlerTests()
        {
            _entityTables = new InMemoryEntityTables();
            _securityService = new Mock<ISecurityService>();
            _emailService = new Mock<IEmailService>();
            _userPasswordResetCommandHandler = new UserPasswordResetCommandHandler(_entityTables, _securityService.Object, _emailService.Object);
            _entityTables.Users.Create();
        }

        [Fact]
        public async Task Handle_WhenTokenDoesNotExist_ThrowsException()
        {
            _securityService
                .Setup(securityService => securityService.ComputeHash("#test@domain.com"))
                .Returns("#email-hash");

            await Assert.ThrowsAsync<NotFoundException>(() => _userPasswordResetCommandHandler.Handle(
                new UserPasswordResetCommand(
                    "#TEST@domain.com",
                    "#token",
                    "#password"
                ),
                default
            ));
        }

        [Fact]
        public async Task Handle_WhenExpiredTokenExist_ThrowsException()
        {
            _securityService
                .Setup(securityService => securityService.ComputeHash("#test@domain.com"))
                .Returns("#email-hash");
            _entityTables.Users.Execute(TableOperation.Insert(new UserPasswordResetTokenEntity
            {
                PartitionKey = "#email-hash".ToEncodedKeyProperty(),
                RowKey = "#token".ToEncodedKeyProperty(),
                EntityType = "UserPasswordResetTokenEntity",
                Expiration = DateTimeOffset.UtcNow.AddDays(-1)
            }));

            await Assert.ThrowsAsync<NotFoundException>(() => _userPasswordResetCommandHandler.Handle(
                new UserPasswordResetCommand(
                    "#TEST@domain.com",
                    "#token",
                    "#password"
                ),
                default
            ));

            Assert.Empty(_entityTables.Users.ExecuteQuery(new TableQuery()));
        }

        [Fact]
        public async Task Handle_WhenUserDoesNotExist_ThrowsException()
        {
            _securityService
                .Setup(securityService => securityService.ComputeHash("#test@domain.com"))
                .Returns("#email-hash");
            _entityTables.Users.Execute(TableOperation.Insert(new UserPasswordResetTokenEntity
            {
                PartitionKey = "#email-hash".ToEncodedKeyProperty(),
                RowKey = "#token".ToEncodedKeyProperty(),
                EntityType = "UserPasswordResetTokenEntity",
                Expiration = DateTimeOffset.UtcNow.AddDays(1)
            }));

            await Assert.ThrowsAsync<NotFoundException>(() => _userPasswordResetCommandHandler.Handle(
                new UserPasswordResetCommand(
                    "#TEST@domain.com",
                    "#token",
                    "#password"
                ),
                default
            ));

            Assert.Empty(_entityTables.Users.ExecuteQuery(new TableQuery()));
        }

        [Fact]
        public async Task Handle_WhenValidTokenExist_ResetsPassword()
        {
            _entityTables.Users.ExecuteBatch(new TableBatchOperation{
                TableOperation.Insert(new UserEntity
                {
                    PartitionKey = "#email-hash".ToEncodedKeyProperty(),
                    RowKey = "details",
                    EntityType = "UserEntity",
                    Id = "#id",
                    Hint = "#hint",
                    PasswordHash = "#old-password-hash",
                    PasswordSalt = "#old-password-salt",
                    IsActive = true
                }),
                TableOperation.Insert(new UserPasswordResetTokenEntity
                {
                    PartitionKey = "#email-hash".ToEncodedKeyProperty(),
                    RowKey = "#token".ToEncodedKeyProperty(),
                    EntityType = "UserPasswordResetTokenEntity",
                    Expiration = DateTimeOffset.UtcNow.AddDays(1)
                })
            });
            _securityService
                .Setup(securityService => securityService.ComputeHash("#test@domain.com"))
                .Returns("#email-hash");
            _securityService
                .Setup(securityService => securityService.GeneratePasswordSalt())
                .Returns("#password-salt");
            _securityService
                .Setup(securityService => securityService.ComputePasswordHash("#password-salt", "#password"))
                .Returns("#password-hash");

            await _userPasswordResetCommandHandler.Handle(
                new UserPasswordResetCommand(
                    "#TEST@domain.com",
                    "#token",
                    "#password"
                ),
                default
            );

            var userEntity = Assert.Single(_entityTables.Users.ExecuteQuery(new TableQuery()));
            Assert.Equal("#email-hash".ToEncodedKeyProperty(), userEntity.PartitionKey);
            Assert.Equal("details", userEntity.RowKey);
            Assert.Equal(6, userEntity.Properties.Count);
            Assert.Equal("UserEntity", userEntity.Properties[nameof(UserEntity.EntityType)].StringValue);
            Assert.Equal("#id", userEntity.Properties[nameof(UserEntity.Id)].StringValue);
            Assert.Equal("#hint", userEntity.Properties[nameof(UserEntity.Hint)].StringValue);
            Assert.Equal("#password-salt", userEntity.Properties[nameof(UserEntity.PasswordSalt)].StringValue);
            Assert.Equal("#password-hash", userEntity.Properties[nameof(UserEntity.PasswordHash)].StringValue);
            Assert.True(userEntity.Properties[nameof(UserEntity.IsActive)].BooleanValue);
        }

        [Fact]
        public async Task Handle_WhenInactiveUserExist_ThrowsException()
        {
            _entityTables.Users.ExecuteBatch(new TableBatchOperation
            {
                TableOperation.Insert(new UserEntity
                {
                    PartitionKey = "#email-hash".ToEncodedKeyProperty(),
                    RowKey = "details",
                    EntityType = "UserEntity",
                    Id = "#id",
                    Hint = "#hint",
                    PasswordHash = "#password-hash",
                    PasswordSalt = "#password-salt",
                    IsActive = false
                }),
                TableOperation.Insert(new UserPasswordResetTokenEntity
                {
                    PartitionKey = "#email-hash".ToEncodedKeyProperty(),
                    RowKey = "#token".ToEncodedKeyProperty(),
                    EntityType = "UserPasswordResetTokenEntity",
                    Expiration = DateTimeOffset.UtcNow.AddDays(1)
                })
            });
            _securityService
                .Setup(securityService => securityService.ComputeHash("#test@domain.com"))
                .Returns("#email-hash");

            await Assert.ThrowsAsync<NotFoundException>(() => _userPasswordResetCommandHandler.Handle(
                new UserPasswordResetCommand(
                    "#TEST@domain.com",
                    "#token",
                    "#password"
                ),
                default
            ));

            var userEntity = Assert.Single(_entityTables.Users.ExecuteQuery(new TableQuery()));
            Assert.Equal("#email-hash".ToEncodedKeyProperty(), userEntity.PartitionKey);
            Assert.Equal("details", userEntity.RowKey);
        }
    }
}