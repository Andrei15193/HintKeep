using System;
using System.Threading.Tasks;
using HintKeep.Exceptions;
using HintKeep.RequestsHandlers.Users.Commands;
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
            _entityTables.Logins.Create();
            _userRegistrationConfirmationCommandHandler = new UserRegistrationConfirmationCommandHandler(_entityTables);
        }

        [Fact]
        public async Task Handle_WhenUserIsPendingConfirmationWithValidToken_ConfirmsTheUser()
        {
            var userId = Guid.NewGuid();
            _entityTables.Logins.ExecuteBatch(new TableBatchOperation
            {
                TableOperation.Insert(new EmailLoginEntity
                {
                    PartitionKey = "test-email",
                    RowKey = "EmailLogin",
                    State = "PendingConfirmation"
                }),
                TableOperation.Insert(new EmailLoginTokenEntity
                {
                    PartitionKey = "test-email",
                    RowKey = "EmailLogin-confirmationToken",
                    Token = "test-token",
                    Created = DateTime.UtcNow
                })
            });

            var command = new UserRegistrationConfirmationCommand
            {
                Email = "test-email",
                ConfirmationToken = "test-token"
            };
            await _userRegistrationConfirmationCommandHandler.Handle(command, default);

            var loginEntity = Assert.Single(_entityTables.Logins.ExecuteQuery(new TableQuery()));
            Assert.Equal("test-email", loginEntity.PartitionKey);
            Assert.Equal("EmailLogin", loginEntity.RowKey);
            Assert.Equal("Confirmed", loginEntity.Properties[nameof(EmailLoginEntity.State)].StringValue);
        }
        
        [Fact]
        public async Task Handle_WhenUserIsPendingConfirmationWithExpiredToken_ThrowsException()
        {
            _entityTables.Logins.ExecuteBatch(new TableBatchOperation
            {
                TableOperation.Insert(new EmailLoginEntity
                {
                    PartitionKey = "test-email",
                    RowKey = "EmailLogin",
                    State = "PendingConfirmation"
                }),
                TableOperation.Insert(new EmailLoginTokenEntity
                {
                    PartitionKey = "test-email",
                    RowKey = "EmailLogin-confirmationToken",
                    Token = "test-token",
                    Created = DateTime.UtcNow.AddDays(-11)
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
            _entityTables.Logins.ExecuteBatch(new TableBatchOperation
            {
                TableOperation.Insert(new EmailLoginEntity
                {
                    PartitionKey = "test-email",
                    RowKey = "EmailLogin",
                    State = "Confirmed"
                }),
                TableOperation.Insert(new EmailLoginTokenEntity
                {
                    PartitionKey = "test-email",
                    RowKey = "EmailLogin-confirmationToken",
                    Token = "test-token",
                    Created = DateTime.UtcNow
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
            _entityTables.Logins.Execute(TableOperation.Insert(new EmailLoginEntity
            {
                PartitionKey = "test-email",
                RowKey = "EmailLogin",
                State = "Confirmed"
            }));

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
            _entityTables.Logins.ExecuteBatch(new TableBatchOperation
            {
                TableOperation.Insert(new EmailLoginEntity
                {
                    PartitionKey = "test-email-2",
                    RowKey = "EmailLogin",
                    State = "Confirmed"
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