using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using HintKeep.Exceptions;
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
    public class CreateUserSessionCommandHandlerTests
    {
        private readonly IEntityTables _entityTables;
        private readonly ISecurityService _securityService;
        private readonly ISessionService _sessionService;
        private readonly IRequestHandler<CreateUserSessionCommand, string> _createUserSessionCommandHandler;

        public CreateUserSessionCommandHandlerTests()
        {
            _entityTables = new InMemoryEntityTables();
            _securityService = Substitute.For<ISecurityService>();
            _sessionService = Substitute.For<ISessionService>();
            _createUserSessionCommandHandler = new CreateUserSessionCommandHandler(_entityTables, _securityService, _sessionService);
            _entityTables.Users.Create();
        }

        [Fact]
        public async Task Handle_WhenUserDoesNotExist_ThrowsException()
        {
            _securityService
                .ComputeHash("#test@domain.com")
                .Returns("#email-hash");

            await Assert.ThrowsAsync<NotFoundException>(() => _createUserSessionCommandHandler.Handle(new CreateUserSessionCommand("#TEST@domain.com", "#test-password"), default));
        }

        [Fact]
        public async Task Handle_WhenInactiveUserExists_ThrowsException()
        {
            _entityTables.Users.Execute(TableOperation.Insert(
                new UserEntity
                {
                    PartitionKey = "#email-hash".ToEncodedKeyProperty(),
                    RowKey = "details",
                    EntityType = "UserEntity",
                    IsActive = false,
                }
            ));
            _securityService
                .ComputeHash("#test@domain.com")
                .Returns("#email-hash");

            await Assert.ThrowsAsync<NotFoundException>(() => _createUserSessionCommandHandler.Handle(new CreateUserSessionCommand("#TEST@domain.com", "#test-password"), default));
        }

        [Fact]
        public async Task Handle_WhenPasswordDoesNotMatch_ThrowsException()
        {
            _entityTables.Users.Execute(TableOperation.Insert(
                new UserEntity
                {
                    PartitionKey = "#email-hash".ToEncodedKeyProperty(),
                    RowKey = "details".ToEncodedKeyProperty(),
                    EntityType = "UserEntity",
                    PasswordSalt = "#password-salt",
                    PasswordHash = "#password-hash",
                    IsActive = true
                }
            ));
            _securityService
                .ComputeHash("#test@domain.com")
                .Returns("#email-hash");
            _securityService
                .ComputePasswordHash("#password-salt", "#test-password")
                .Returns("#password-hash-not-matching");

            var exception = await Assert.ThrowsAsync<ValidationException>(() => _createUserSessionCommandHandler.Handle(new CreateUserSessionCommand("#TEST@domain.com", "#test-password"), default));
            Assert.Equal("errors.login.invalidCredentials", exception.ValidationResult.ErrorMessage);
        }

        [Fact]
        public async Task Handle_WhenPasswordMatches_ReturnsJsonWebTokenAndSetsLastLoginTime()
        {
            _entityTables.Users.Execute(TableOperation.Insert(
                new UserEntity
                {
                    PartitionKey = "#email-hash".ToEncodedKeyProperty(),
                    RowKey = "details".ToEncodedKeyProperty(),
                    EntityType = "UserEntity",
                    Id = "#user-id",
                    Role = "#user-role",
                    PasswordSalt = "#password-salt",
                    PasswordHash = "#password-hash",
                    IsActive = true
                }
            ));
            _securityService
                .ComputePasswordHash("#password-salt", "#test-password")
                .Returns("#password-hash");
            _securityService
                .ComputeHash("#test@domain.com")
                .Returns("#email-hash");
            _sessionService
                .CreateJsonWebToken("#user-id", "#user-role")
                .Returns("#json-web-token");

            var jsonWebToken = await _createUserSessionCommandHandler.Handle(new CreateUserSessionCommand("#TEST@domain.com", "#test-password"), default);
            Assert.Equal("#json-web-token", jsonWebToken);

            var userEntity = Assert.Single(_entityTables.Users.ExecuteQuery(new TableQuery<UserEntity>()));
            Assert.Equal("#email-hash".ToEncodedKeyProperty(), userEntity.PartitionKey);
            Assert.Equal("details".ToEncodedKeyProperty(), userEntity.RowKey);
            Assert.NotNull(userEntity.LastLoginTime);
            Assert.True(DateTimeOffset.UtcNow.AddMinutes(-1) <= userEntity.LastLoginTime && userEntity.LastLoginTime <= DateTimeOffset.UtcNow.AddMinutes(1));
        }
    }
}