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
                    EntityType = "EmailLoginEntity",
                    PartitionKey = "#test-email".ToEncodedKeyProperty(),
                    RowKey = "EmailLogin".ToEncodedKeyProperty(),
                    State = "PendingConfirmation"
                }),
                TableOperation.Insert(new EmailLoginTokenEntity
                {
                    EntityType = "EmailLoginTokenEntity",
                    PartitionKey = "#test-email".ToEncodedKeyProperty(),
                    RowKey = "EmailLogin-confirmationToken".ToEncodedKeyProperty(),
                    Token = "#test-token",
                    Created = DateTime.UtcNow
                })
            });

            var command = new UserRegistrationConfirmationCommand
            {
                Email = "#test-email",
                ConfirmationToken = "#test-token"
            };
            await _userRegistrationConfirmationCommandHandler.Handle(command, default);

            var loginEntity = Assert.Single(_entityTables.Logins.ExecuteQuery(new TableQuery()));
            Assert.Equal("EmailLoginEntity", loginEntity.Properties[nameof(HintKeepTableEntity.EntityType)].StringValue);
            Assert.Equal("#test-email", loginEntity.PartitionKey.FromEncodedKeyProperty());
            Assert.Equal("EmailLogin", loginEntity.RowKey.FromEncodedKeyProperty());
            Assert.Equal("Confirmed", loginEntity.Properties[nameof(EmailLoginEntity.State)].StringValue);
        }
        
        [Fact]
        public async Task Handle_WhenUserIsPendingConfirmationWithExpiredToken_ThrowsException()
        {
            _entityTables.Logins.ExecuteBatch(new TableBatchOperation
            {
                TableOperation.Insert(new EmailLoginEntity
                {
                    EntityType = "EmailLoginEntity",
                    PartitionKey = "#test-email".ToEncodedKeyProperty(),
                    RowKey = "EmailLogin".ToEncodedKeyProperty(),
                    State = "PendingConfirmation"
                }),
                TableOperation.Insert(new EmailLoginTokenEntity
                {
                    EntityType = "EmailLoginTokenEntity",
                    PartitionKey = "#test-email".ToEncodedKeyProperty(),
                    RowKey = "EmailLogin-confirmationToken".ToEncodedKeyProperty(),
                    Token = "#test-token",
                    Created = DateTime.UtcNow.AddDays(-11)
                })
            });

            var command = new UserRegistrationConfirmationCommand
            {
                Email = "#test-email",
                ConfirmationToken = "#test-token"
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
                    EntityType = "EmailLoginEntity",
                    PartitionKey = "#test-email".ToEncodedKeyProperty(),
                    RowKey = "EmailLogin".ToEncodedKeyProperty(),
                    State = "Confirmed"
                }),
                TableOperation.Insert(new EmailLoginTokenEntity
                {
                    EntityType = "EmailLoginTokenEntity",
                    PartitionKey = "#test-email".ToEncodedKeyProperty(),
                    RowKey = "EmailLogin-confirmationToken".ToEncodedKeyProperty(),
                    Token = "#test-token",
                    Created = DateTime.UtcNow
                })
            });

            var command = new UserRegistrationConfirmationCommand
            {
                Email = "#test-email",
                ConfirmationToken = "#test-token"
            };

            var exception = await Assert.ThrowsAsync<PreconditionFailedException>(() => _userRegistrationConfirmationCommandHandler.Handle(command, default));
            Assert.Empty(exception.Message);
        }
        
        [Fact]
        public async Task Handle_WhenThereIsNoConfirmationToken_ThrowsException()
        {
            _entityTables.Logins.Execute(TableOperation.Insert(new EmailLoginEntity
            {
                EntityType = "EmailLoginEntity",
                PartitionKey = "#test-email".ToEncodedKeyProperty(),
                RowKey = "EmailLogin".ToEncodedKeyProperty(),
                State = "Confirmed"
            }));

            var command = new UserRegistrationConfirmationCommand
            {
                Email = "#test-email",
                ConfirmationToken = "#test-token"
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
                    EntityType = "EmailLoginEntity",
                    PartitionKey = "#test-email-2".ToEncodedKeyProperty(),
                    RowKey = "EmailLogin".ToEncodedKeyProperty(),
                    State = "Confirmed"
                })
            });

            var command = new UserRegistrationConfirmationCommand
            {
                Email = "#test-email",
                ConfirmationToken = "#test-token"
            };

            var exception = await Assert.ThrowsAsync<PreconditionFailedException>(() => _userRegistrationConfirmationCommandHandler.Handle(command, default));
            Assert.Empty(exception.Message);
        }
    }
}