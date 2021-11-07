using System;
using System.Threading.Tasks;
using HintKeep.Exceptions;
using HintKeep.Requests.Users.Commands;
using HintKeep.RequestsHandlers.Users.Commands;
using HintKeep.Storage;
using HintKeep.Storage.Entities;
using HintKeep.Tests.Stubs;
using MediatR;
using Microsoft.Azure.Cosmos.Table;
using Xunit;

namespace HintKeep.Tests.Unit.RequestsHandlers.Users.Commands
{
    public class ConfirmUserCommandHandlerTests
    {
        private readonly IEntityTables _entityTables;
        private readonly IRequestHandler<ConfirmUserCommand> _confirmUserCommandHandler;

        public ConfirmUserCommandHandlerTests()
        {
            _entityTables = new InMemoryEntityTables();
            _confirmUserCommandHandler = new ConfirmUserCommandHandler(_entityTables);
            _entityTables.Users.Create();
        }

        [Fact]
        public async Task Handle_WhenValidTokenExists_ActivatesUser()
        {
            _entityTables.Users.ExecuteBatch(new TableBatchOperation
            {
                TableOperation.Insert(new UserEntity
                {
                    PartitionKey = "#test@domain.com".ToEncodedKeyProperty(),
                    RowKey = "details",
                    EntityType = "UserEntity",
                    Id = "#user-id",
                    Email = "#TEST@domain.com",
                    Hint = "#test-hint",
                    PasswordSalt = "#password-salt",
                    PasswordHash = "#password-hash",
                    IsActive = false
                }),
                TableOperation.Insert(new UserActivationTokenEntity
                {
                    PartitionKey = "#test@domain.com".ToEncodedKeyProperty(),
                    RowKey = "#test-token".ToEncodedKeyProperty(),
                    EntityType = "UserActivationTokenEntity",
                    Expiration = DateTimeOffset.UtcNow.AddDays(1)
                })
            });

            await _confirmUserCommandHandler.Handle(new ConfirmUserCommand { Token = "#test-token" }, default);

            var userEntity = Assert.Single(_entityTables.Users.ExecuteQuery(new TableQuery()));
            Assert.Equal("#test@domain.com".ToEncodedKeyProperty(), userEntity.PartitionKey);
            Assert.Equal("details", userEntity.RowKey);
            Assert.Equal(7, userEntity.Properties.Count);
            Assert.Equal("UserEntity", userEntity.Properties[nameof(HintKeepTableEntity.EntityType)].StringValue);
            Assert.NotEmpty(userEntity.Properties[nameof(UserEntity.Id)].StringValue);
            Assert.Equal("#TEST@domain.com", userEntity.Properties[nameof(UserEntity.Email)].StringValue);
            Assert.Equal("#test-hint", userEntity.Properties[nameof(UserEntity.Hint)].StringValue);
            Assert.Equal("#password-salt", userEntity.Properties[nameof(UserEntity.PasswordSalt)].StringValue);
            Assert.Equal("#password-hash", userEntity.Properties[nameof(UserEntity.PasswordHash)].StringValue);
            Assert.True(userEntity.Properties[nameof(UserEntity.IsActive)].BooleanValue);
        }

        [Fact]
        public async Task Handle_WhenTokenDoesNotExist_ThrowsException()
        {
            await Assert.ThrowsAsync<NotFoundException>(() => _confirmUserCommandHandler.Handle(new ConfirmUserCommand { Token = "#test-token" }, default));
        }

        [Fact]
        public async Task Handle_WhenExpiredTokenExists_ThrowsException()
        {
            _entityTables.Users.ExecuteBatch(new TableBatchOperation
            {
                TableOperation.Insert(new UserEntity
                {
                    PartitionKey = "#test@domain.com".ToEncodedKeyProperty(),
                    RowKey = "details",
                    EntityType = "UserEntity",
                    Id = "#user-id",
                    Email = "#TEST@domain.com",
                    Hint = "#test-hint",
                    PasswordSalt = "#password-salt",
                    PasswordHash = "#password-hash",
                    IsActive = false
                }),
                TableOperation.Insert(new UserActivationTokenEntity
                {
                    PartitionKey = "#test@domain.com".ToEncodedKeyProperty(),
                    RowKey = "#test-token".ToEncodedKeyProperty(),
                    EntityType = "UserActivationTokenEntity",
                    Expiration = DateTimeOffset.UtcNow.AddDays(-1)
                })
            });

            await Assert.ThrowsAsync<NotFoundException>(() => _confirmUserCommandHandler.Handle(new ConfirmUserCommand { Token = "#test-token" }, default));

            Assert.Empty(_entityTables.Users.ExecuteQuery(new TableQuery()));
        }
    }
}