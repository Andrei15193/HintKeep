using System;
using System.Threading.Tasks;
using HintKeep.Exceptions;
using HintKeep.RequestHandlers.Users.Commands;
using HintKeep.Requests.Users.Commands;
using HintKeep.Storage;
using HintKeep.Storage.Entities;
using HintKeep.Tests.Stubs;
using MediatR;
using Microsoft.Azure.Cosmos.Table;
using Xunit;

namespace HintKeep.Tests.Unit.RequestsHandlers.Users.Commands
{
    public class UserRegistrationConfirmationCommandHandlerTests
    {
        private readonly IEntityTables _entityTables;
        private readonly IRequestHandler<UserRegistrationConfirmationCommand> _userRegistrationConfirmationCommandHandler;

        public UserRegistrationConfirmationCommandHandlerTests()
        {
            _entityTables = new InMemoryEntityTables();
            _entityTables.Users.Create();
            _userRegistrationConfirmationCommandHandler = new UserRegistrationConfirmationCommandHandler(_entityTables);
        }

        [Fact]
        public async Task Handle_WhenUserIsPendingConfirmationWithValidToken_ConfirmsTheUser()
        {
            _entityTables.Users.ExecuteBatch(new TableBatchOperation
            {
                TableOperation.Insert(new UserEntity
                {
                    PartitionKey = "test-email",
                    RowKey = "user",
                    State = (int)UserState.PendingConfirmation
                }),
                TableOperation.Insert(new TokenEntity
                {
                    PartitionKey = "test-email",
                    RowKey = "confirmation_tokens-test-token",
                    Token = "test-token",
                    Intent = (int)TokenIntent.ConfirmUserRegistration,
                    Created = DateTime.UtcNow.AddMinutes(-1)
                })
            });

            var command = new UserRegistrationConfirmationCommand
            {
                Email = "test-email",
                ConfirmationToken = "test-token"
            };
            await _userRegistrationConfirmationCommandHandler.Handle(command, default);

            var entities = _entityTables.Users.ExecuteQuery(new TableQuery());
            var userEntity = Assert.Single(entities);
            Assert.Equal("test-email", userEntity.PartitionKey);
            Assert.Equal("user", userEntity.RowKey);
            Assert.Equal(UserState.Confirmed, (UserState)userEntity.Properties[nameof(UserEntity.State)].Int32Value);
        }
        
        [Fact]
        public async Task Handle_WhenUserIsPendingConfirmationWithExpiredToken_ThrowsException()
        {
            _entityTables.Users.ExecuteBatch(new TableBatchOperation
            {
                TableOperation.Insert(new UserEntity
                {
                    PartitionKey = "test-email",
                    RowKey = "user",
                    State = (int)UserState.PendingConfirmation
                }),
                TableOperation.Insert(new TokenEntity
                {
                    PartitionKey = "test-email",
                    RowKey = "confirmation_tokens-test-token",
                    Token = "test-token",
                    Intent = (int)TokenIntent.ConfirmUserRegistration,
                    Created = DateTime.UtcNow.AddDays(-2)
                })
            });

            var command = new UserRegistrationConfirmationCommand
            {
                Email = "test-email",
                ConfirmationToken = "test-token"
            };

            var exception = await Assert.ThrowsAsync<PreconditionFailedException>(() => _userRegistrationConfirmationCommandHandler.Handle(command, default));
            Assert.Empty(exception.Message);
        }
        
        [Fact]
        public async Task Handle_WhenUserIsConfirmed_ThrowsException()
        {
            _entityTables.Users.ExecuteBatch(new TableBatchOperation
            {
                TableOperation.Insert(new UserEntity
                {
                    PartitionKey = "test-email",
                    RowKey = "user",
                    State = (int)UserState.Confirmed
                }),
                TableOperation.Insert(new TokenEntity
                {
                    PartitionKey = "test-email",
                    RowKey = "confirmation_tokens-test-token",
                    Token = "test-token",
                    Intent = (int)TokenIntent.ConfirmUserRegistration,
                    Created = DateTime.UtcNow.AddMinutes(-1)
                })
            });

            var command = new UserRegistrationConfirmationCommand
            {
                Email = "test-email",
                ConfirmationToken = "test-token"
            };

            var exception = await Assert.ThrowsAsync<PreconditionFailedException>(() => _userRegistrationConfirmationCommandHandler.Handle(command, default));
            Assert.Empty(exception.Message);
        }
        
        [Fact]
        public async Task Handle_WhenThereIsNoConfirmationToken_ThrowsException()
        {
            _entityTables.Users.ExecuteBatch(new TableBatchOperation
            {
                TableOperation.Insert(new UserEntity
                {
                    PartitionKey = "test-email",
                    RowKey = "user",
                    State = (int)UserState.Confirmed
                })
            });

            var command = new UserRegistrationConfirmationCommand
            {
                Email = "test-email",
                ConfirmationToken = "test-token"
            };

            var exception = await Assert.ThrowsAsync<PreconditionFailedException>(() => _userRegistrationConfirmationCommandHandler.Handle(command, default));
            Assert.Empty(exception.Message);
        }
        
        [Fact]
        public async Task Handle_WhenThereIsNoUser_ThrowsException()
        {
            _entityTables.Users.ExecuteBatch(new TableBatchOperation
            {
                TableOperation.Insert(new UserEntity
                {
                    PartitionKey = "test-email-2",
                    RowKey = "user",
                    State = (int)UserState.Confirmed
                })
            });

            var command = new UserRegistrationConfirmationCommand
            {
                Email = "test-email",
                ConfirmationToken = "test-token"
            };

            var exception = await Assert.ThrowsAsync<PreconditionFailedException>(() => _userRegistrationConfirmationCommandHandler.Handle(command, default));
            Assert.Empty(exception.Message);
        }
    }
}