using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Timers;
using ToDoApp.API.Controllers.Users;
using ToDoApp.Application.UserManagement.Interfaces;
using ToDoApp.Domain.Entities.User;
using Xunit;

namespace ToDoApp.Tests.Controllers
{
    public class UsersControllerTests
    {
        private readonly Mock<IUserService> _mockUserService;
        private readonly UsersController _controller;

        public UsersControllerTests()
        {
            _mockUserService = new Mock<IUserService>();
            _controller = new UsersController(_mockUserService.Object);
        }

        [Fact]
        public async Task GetUsers_ReturnsOk_WithListOfUsers()
        {
            // Arrange
            var users = new List<User>
            {
                new User { Id = Guid.NewGuid(), FirstName = "Alice" },
                new User { Id = Guid.NewGuid(), FirstName = "Bob" }
            };
            _mockUserService.Setup(s => s.GetUsersAsync()).ReturnsAsync(users);

            // Act
            var result = await _controller.GetUsers();

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var returned = Assert.IsAssignableFrom<IEnumerable<User>>(ok.Value);
            Assert.Equal(2, ((List<User>)returned).Count);
        }

        [Fact]
        public async Task GetUsers_ReturnsOk_WithEmptyList()
        {
            // Arrange
            _mockUserService.Setup(s => s.GetUsersAsync()).ReturnsAsync(new List<User>());

            // Act
            var result = await _controller.GetUsers();

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var returned = Assert.IsAssignableFrom<IEnumerable<User>>(ok.Value);
            Assert.Empty(returned);
        }


        [Fact]
        public async Task GetUserById_ReturnsOk_WhenUserExists()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new User { Id = userId, FirstName = "Alice" };
            _mockUserService.Setup(s => s.GetUserByIdAsync(userId)).ReturnsAsync(user);

            // Act
            var result = await _controller.GetUserById(userId);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var returned = Assert.IsType<User>(ok.Value);
            Assert.Equal(userId, returned.Id);
        }

        [Fact]
        public async Task GetUserById_ReturnsNotFound_WhenUserDoesNotExist()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _mockUserService.Setup(s => s.GetUserByIdAsync(userId)).ReturnsAsync((User?)null);

            // Act
            var result = await _controller.GetUserById(userId);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }


        [Fact]
        public async Task UpdateUser_ReturnsOk_WhenUserUpdated()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var inputUser = new User { Id = userId, FirstName = "Alice Updated" };
            _mockUserService.Setup(s => s.UpdateUserAsync(userId, inputUser)).ReturnsAsync(inputUser);

            // Act
            var result = await _controller.UpdateUser(userId, inputUser);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var returned = Assert.IsType<User>(ok.Value);
            Assert.Equal("Alice Updated", returned.FirstName);
        }

        [Fact]
        public async Task UpdateUser_ReturnsNotFound_WhenUserDoesNotExist()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var inputUser = new User { Id = userId, FirstName = "Ghost" };
            _mockUserService.Setup(s => s.UpdateUserAsync(userId, inputUser)).ReturnsAsync((User?)null);

            // Act
            var result = await _controller.UpdateUser(userId, inputUser);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }


        [Fact]
        public async Task DeleteUser_ReturnsNoContent_WhenUserDeleted()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _mockUserService.Setup(s => s.DeleteUserAsync(userId)).ReturnsAsync(true);

            // Act
            var result = await _controller.DeleteUser(userId);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteUser_ReturnsNotFound_WhenUserDoesNotExist()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _mockUserService.Setup(s => s.DeleteUserAsync(userId)).ReturnsAsync(false);

            // Act
            var result = await _controller.DeleteUser(userId);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task IsEmailUnique_ReturnsTrue_WhenEmailIsUnique()
        {
            // Arrange
            var email = "unique@example.com";
            _mockUserService.Setup(s => s.IsEmailUniqueAsync(email)).ReturnsAsync(true);

            // Act
            var result = await _controller.IsEmailUnique(email);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var val = ok.Value!;
            Assert.True((bool)val.GetType().GetProperty("isUnique")!.GetValue(val)!);
            Assert.Equal(email, (string)val.GetType().GetProperty("email")!.GetValue(val)!);
        }

        [Fact]
        public async Task IsEmailUnique_ReturnsFalse_WhenEmailIsTaken()
        {
            // Arrange
            var email = "taken@example.com";
            _mockUserService.Setup(s => s.IsEmailUniqueAsync(email)).ReturnsAsync(false);

            // Act
            var result = await _controller.IsEmailUnique(email);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var val = ok.Value!;
            Assert.False((bool)val.GetType().GetProperty("isUnique")!.GetValue(val)!);
        }

        [Fact]
        public async Task GetUserById_CallsServiceOnce_WithCorrectId()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _mockUserService.Setup(s => s.GetUserByIdAsync(userId)).ReturnsAsync((User?)null);

            // Act
            await _controller.GetUserById(userId);

            // Assert
            _mockUserService.Verify(s => s.GetUserByIdAsync(userId), Times.Once);
        }

        [Fact]
        public async Task DeleteUser_CallsServiceOnce_WithCorrectId()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _mockUserService.Setup(s => s.DeleteUserAsync(userId)).ReturnsAsync(true);

            // Act
            await _controller.DeleteUser(userId);

            // Assert
            _mockUserService.Verify(s => s.DeleteUserAsync(userId), Times.Once);
        }
    }
}