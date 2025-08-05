using Application.Features.Auth.Commands;
using FluentAssertions;
using Infrastructure.Identity;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging; // Eklendi
using Microsoft.Extensions.Options; // Eklendi
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Application.UnitTests.Features.Auth.Commands
{
    public class SetPasswordForCustomerCommandHandlerTests
    {
        private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
        private readonly SetPasswordForCustomerCommandHandler _handler;

        public SetPasswordForCustomerCommandHandlerTests()
        {
            // UserManager'ın mock'lanması, constructor'ının birçok bağımlılığı olması nedeniyle karmaşıktır.
            // Null uyarılarını gidermek için, null yerine Mock.Of<T>() ile sahte (dummy) mock'lar oluşturuyoruz.
            var store = new Mock<IUserStore<ApplicationUser>>();
            _mockUserManager = new Mock<UserManager<ApplicationUser>>(
                store.Object,
                Mock.Of<IOptions<IdentityOptions>>(),
                Mock.Of<IPasswordHasher<ApplicationUser>>(),
                Array.Empty<IUserValidator<ApplicationUser>>(),
                Array.Empty<IPasswordValidator<ApplicationUser>>(),
                Mock.Of<ILookupNormalizer>(),
                Mock.Of<IdentityErrorDescriber>(),
                Mock.Of<IServiceProvider>(),
                Mock.Of<ILogger<UserManager<ApplicationUser>>>());
            
            _handler = new SetPasswordForCustomerCommandHandler(_mockUserManager.Object);
        }

        [Fact]
        public async Task Handle_GivenValidRequest_ShouldSucceed()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new ApplicationUser { Id = userId, EmailConfirmed = false };
            var command = new SetPasswordForCustomerCommand
            {
                UserId = userId.ToString(),
                Token = "valid_token",
                NewPassword = "NewPassword123!"
            };

            _mockUserManager.Setup(x => x.FindByIdAsync(userId.ToString()))
                .ReturnsAsync(user);

            _mockUserManager.Setup(x => x.ResetPasswordAsync(user, command.Token, command.NewPassword))
                .ReturnsAsync(IdentityResult.Success);
            
            _mockUserManager.Setup(x => x.UpdateAsync(user))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().Be(Unit.Value);
            user.EmailConfirmed.Should().BeTrue();

            // UserManager'daki metotların doğru şekilde çağrıldığını doğrula
            _mockUserManager.Verify(x => x.FindByIdAsync(userId.ToString()), Times.Once);
            _mockUserManager.Verify(x => x.ResetPasswordAsync(user, command.Token, command.NewPassword), Times.Once);
            _mockUserManager.Verify(x => x.UpdateAsync(user), Times.Once);
        }

        [Fact]
        public async Task Handle_GivenInvalidUser_ShouldThrowNotFoundException()
        {
            // Arrange
            var command = new SetPasswordForCustomerCommand
            {
                UserId = Guid.NewGuid().ToString(),
                Token = "any_token",
                NewPassword = "any_password"
            };

            _mockUserManager.Setup(x => x.FindByIdAsync(It.IsAny<string>()))
                .ReturnsAsync((ApplicationUser)null);

            // Act
            Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<Application.Common.Exceptions.NotFoundException>();
        }
    }
}
