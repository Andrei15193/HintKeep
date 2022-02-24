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
    public class UserRequestHintCommandHandlerTests
    {
        private readonly IEntityTables _entityTables;
        private readonly Mock<ISecurityService> _securityService;
        private readonly Mock<IEmailService> _emailService;
        private readonly IRequestHandler<UserRequestHintCommand> _userRequestHintCommandHandler;

        public UserRequestHintCommandHandlerTests()
        {
            _entityTables = new InMemoryEntityTables();
            _securityService = new Mock<ISecurityService>();
            _emailService = new Mock<IEmailService>();
            _userRequestHintCommandHandler = new UserRequestHintCommandHandler(_entityTables, _securityService.Object, _emailService.Object);
            _entityTables.Users.Create();
        }

        [Fact]
        public async Task Handle_WhenUserDoesNotExist_ThrowsException()
        {
            _securityService
                .Setup(securityService => securityService.ComputeHash("#test@domain.com"))
                .Returns("#email-hash");

            await Assert.ThrowsAsync<NotFoundException>(() => _userRequestHintCommandHandler.Handle(new UserRequestHintCommand { Email = "#TEST@domain.com" }, default));
        }

        [Fact]
        public async Task Handle_WhenInactiveUserExist_ThrowsException()
        {
            _entityTables.Users.Execute(TableOperation.Insert(new UserEntity
            {
                PartitionKey = "#email-hash".ToEncodedKeyProperty(),
                RowKey = "details".ToEncodedKeyProperty(),
                EntityType = "UserEntity",
                IsActive = false
            }));
            _securityService
                .Setup(securityService => securityService.ComputeHash("#test@domain.com"))
                .Returns("#email-hash");

            await Assert.ThrowsAsync<NotFoundException>(() => _userRequestHintCommandHandler.Handle(new UserRequestHintCommand { Email = "#TEST@domain.com" }, default));
        }

        [Fact]
        public async Task Handle_WhenActiveUserExist_SendsEmailNotification()
        {
            _entityTables.Users.Execute(TableOperation.Insert(new UserEntity
            {
                PartitionKey = "#email-hash".ToEncodedKeyProperty(),
                RowKey = "details".ToEncodedKeyProperty(),
                EntityType = "UserEntity",
                Hint = "#hint",
                IsActive = true
            }));
            _securityService
                .Setup(securityService => securityService.ComputeHash("#test@domain.com"))
                .Returns("#email-hash");

            await _userRequestHintCommandHandler.Handle(new UserRequestHintCommand { Email = "#TEST@domain.com" }, default);

            _emailService.Verify(emailService => emailService.SendAsync("#TEST@domain.com", "HintKeep - Account Hint", It.IsRegex("#hint")), Times.Once);
            _emailService.VerifyNoOtherCalls();
        }
    }
}