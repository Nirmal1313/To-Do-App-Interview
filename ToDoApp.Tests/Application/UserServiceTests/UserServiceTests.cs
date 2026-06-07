using Microsoft.Extensions.Logging;
using Moq;
using ToDoApp.Application.UserManagement.Services;
using ToDoApp.Domain.Entities.User;
using ToDoApp.Domain.Interfaces.UserManagment;
using Xunit;

namespace ToDoApp.Tests.Application.UserServiceTests
{
    public class UserServiceTests
    {
        private readonly Mock<IUserRepository> _repoMock;
        private readonly Mock<ILogger<UserService>> _loggerMock;
        private readonly UserService _service;

        public UserServiceTests()
        {
            _repoMock = new Mock<IUserRepository>();
            _loggerMock = new Mock<ILogger<UserService>>();
            _service = new UserService(_repoMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task GetUsersAsync_ReturnsAllUsers()
        {
            // Arrange
            var users = new List<User>
            {
                new() { Id = Guid.NewGuid(), FirstName = "Alice" },
                new() { Id = Guid.NewGuid(), FirstName = "Bob" }
            };
            _repoMock.Setup(r => r.GetUsersAsync()).ReturnsAsync(users);

            // Act
            var result = await _service.GetUsersAsync();

            // Assert
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetUsersAsync_WhenEmpty_ReturnsEmptyList()
        {
            // Arrange
            _repoMock.Setup(r => r.GetUsersAsync()).ReturnsAsync(new List<User>());

            // Act
            var result = await _service.GetUsersAsync();

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetUserByIdAsync_WhenFound_ReturnsUser()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new User { Id = userId, FirstName = "Alice" };
            _repoMock.Setup(r => r.GetUserByIdAsync(userId)).ReturnsAsync(user);

            // Act
            var result = await _service.GetUserByIdAsync(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(userId, result!.Id);
        }

        [Fact]
        public async Task GetUserByIdAsync_WhenNotFound_ReturnsNull()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _repoMock.Setup(r => r.GetUserByIdAsync(userId)).ReturnsAsync((User?)null);

            // Act
            var result = await _service.GetUserByIdAsync(userId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task CreateUserAsync_ReturnsCreatedUser()
        {
            // Arrange
            var user = new User { Email = "test@test.com" };
            _repoMock.Setup(r => r.CreateUserAsync(user)).ReturnsAsync(user);

            // Act
            var result = await _service.CreateUserAsync(user);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("test@test.com", result.Email);
        }

        [Fact]
        public async Task CreateUserAsync_CallsRepositoryOnce()
        {
            // Arrange
            var user = new User { Email = "test@test.com" };
            _repoMock.Setup(r => r.CreateUserAsync(user)).ReturnsAsync(user);

            // Act
            await _service.CreateUserAsync(user);

            // Assert
            _repoMock.Verify(r => r.CreateUserAsync(user), Times.Once);
        }

        [Fact]
        public async Task UpdateUserAsync_WhenFound_ReturnsUpdatedUser()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var existing = new User { Id = userId, FirstName = "Old" };
            var updated = new User { Id = userId, FirstName = "New" };

            _repoMock.Setup(r => r.GetUserByIdAsync(userId)).ReturnsAsync(existing);
            _repoMock.Setup(r => r.UpdateUserAsync(It.IsAny<User>())).ReturnsAsync(updated);

            // Act
            var result = await _service.UpdateUserAsync(userId, new User { FirstName = "New" });

            // Assert
            Assert.NotNull(result);
            Assert.Equal("New", result!.FirstName);
        }

        [Fact]
        public async Task UpdateUserAsync_WhenNotFound_ReturnsNull()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _repoMock.Setup(r => r.GetUserByIdAsync(userId)).ReturnsAsync((User?)null);

            // Act
            var result = await _service.UpdateUserAsync(userId, new User());

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task UpdateUserAsync_PreservesOriginalId()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var existing = new User { Id = userId };
            User? captured = null;

            _repoMock.Setup(r => r.GetUserByIdAsync(userId)).ReturnsAsync(existing);
            _repoMock.Setup(r => r.UpdateUserAsync(It.IsAny<User>())).Callback<User>(u => captured = u).ReturnsAsync((User u) => u);

            // Act
            await _service.UpdateUserAsync(userId, new User { Id = Guid.NewGuid() });

            // Assert
            Assert.Equal(userId, captured!.Id);
        }

        [Fact]
        public async Task DeleteUserAsync_WhenFound_ReturnsTrue()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _repoMock.Setup(r => r.DeleteUserAsync(userId)).ReturnsAsync(true);

            // Act
            var result = await _service.DeleteUserAsync(userId);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task DeleteUserAsync_WhenNotFound_ReturnsFalse()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _repoMock.Setup(r => r.DeleteUserAsync(userId)).ReturnsAsync(false);

            // Act
            var result = await _service.DeleteUserAsync(userId);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task IsEmailUniqueAsync_WhenEmailNotInUse_ReturnsTrue()
        {
            // Arrange
            _repoMock.Setup(r => r.GetUsersByEmailAsync("new@test.com")).ReturnsAsync((User?)null);

            // Act
            var result = await _service.IsEmailUniqueAsync("new@test.com");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task IsEmailUniqueAsync_WhenEmailTaken_ReturnsFalse()
        {
            // Arrange
            _repoMock.Setup(r => r.GetUsersByEmailAsync("taken@test.com"))
                .ReturnsAsync(new User { Email = "taken@test.com" });

            // Act
            var result = await _service.IsEmailUniqueAsync("taken@test.com");

            // Assert
            Assert.False(result);
        }
    }
}
