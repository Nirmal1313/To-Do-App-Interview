using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ToDoApp.Domain.Entities.User;
using ToDoApp.Infrastructure.Data;
using ToDoApp.Infrastructure.Repositories.UserManagement;
using Xunit;

namespace ToDoApp.Tests.Infrastructure.UserRepositoryTests
{
    public class UserRepositoryTests
    {
        private AppDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new AppDbContext(options);
        }

        private static User MakeUser(string email = "user@test.com") => new()
        {
            FirstName = "Test",
            LastName = "User",
            Email = email,
            PasswordHash = new PasswordHasher<User>().HashPassword(null!, "pass")
        };

        [Fact]
        public async Task CreateUserAsync_SavesUser()
        {
            // Arrange
            var context = GetDbContext();
            var repo = new UserRepository(context);
            var user = MakeUser();

            // Act
            await repo.CreateUserAsync(user);

            // Assert
            var stored = await context.Users.FirstOrDefaultAsync();
            Assert.NotNull(stored);
            Assert.Equal("user@test.com", stored!.Email);
        }

        [Fact]
        public async Task CreateUserAsync_AssignsNewId()
        {
            // Arrange
            var context = GetDbContext();
            var repo = new UserRepository(context);
            var user = MakeUser();
            user.Id = Guid.Empty;

            // Act
            var result = await repo.CreateUserAsync(user);

            // Assert
            Assert.NotEqual(Guid.Empty, result.Id);
        }

        [Fact]
        public async Task GetUsersAsync_ReturnsAllUsers()
        {
            // Arrange
            var context = GetDbContext();
            var repo = new UserRepository(context);
            await context.Users.AddRangeAsync(MakeUser("a@test.com"), MakeUser("b@test.com"));
            await context.SaveChangesAsync();

            // Act
            var result = await repo.GetUsersAsync();

            // Assert
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetUsersAsync_WhenEmpty_ReturnsEmptyList()
        {
            // Arrange
            var context = GetDbContext();
            var repo = new UserRepository(context);

            // Act
            var result = await repo.GetUsersAsync();

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetUserByIdAsync_WhenFound_ReturnsUser()
        {
            // Arrange
            var context = GetDbContext();
            var repo = new UserRepository(context);
            var user = MakeUser();
            await context.Users.AddAsync(user);
            await context.SaveChangesAsync();

            // Act
            var result = await repo.GetUserByIdAsync(user.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(user.Id, result!.Id);
        }

        [Fact]
        public async Task GetUserByIdAsync_WhenNotFound_ReturnsNull()
        {
            // Arrange
            var context = GetDbContext();
            var repo = new UserRepository(context);

            // Act
            var result = await repo.GetUserByIdAsync(Guid.NewGuid());

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetUsersByEmailAsync_WhenFound_ReturnsUser()
        {
            // Arrange
            var context = GetDbContext();
            var repo = new UserRepository(context);
            var user = MakeUser("find@test.com");
            await context.Users.AddAsync(user);
            await context.SaveChangesAsync();

            // Act
            var result = await repo.GetUsersByEmailAsync("find@test.com");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("find@test.com", result!.Email);
        }

        [Fact]
        public async Task GetUsersByEmailAsync_IsCaseInsensitive()
        {
            // Arrange
            var context = GetDbContext();
            var repo = new UserRepository(context);
            var user = MakeUser("case@test.com");
            await context.Users.AddAsync(user);
            await context.SaveChangesAsync();

            // Act
            var result = await repo.GetUsersByEmailAsync("CASE@TEST.COM");

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task GetUsersByEmailAsync_WhenNotFound_ReturnsNull()
        {
            // Arrange
            var context = GetDbContext();
            var repo = new UserRepository(context);

            // Act
            var result = await repo.GetUsersByEmailAsync("nobody@test.com");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task UpdateUserAsync_WhenFound_UpdatesFields()
        {
            // Arrange
            var context = GetDbContext();
            var repo = new UserRepository(context);
            var user = MakeUser();
            await context.Users.AddAsync(user);
            await context.SaveChangesAsync();

            // Act
            var result = await repo.UpdateUserAsync(new User
            {
                Id = user.Id,
                FirstName = "Updated",
                LastName = "Name",
                Email = "updated@test.com"
            });

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Updated", result!.FirstName);
            Assert.Equal("updated@test.com", result.Email);
        }

        [Fact]
        public async Task UpdateUserAsync_WhenNotFound_ReturnsNull()
        {
            // Arrange
            var context = GetDbContext();
            var repo = new UserRepository(context);

            // Act
            var result = await repo.UpdateUserAsync(new User { Id = Guid.NewGuid() });

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task DeleteUserAsync_WhenFound_RemovesUser_AndReturnsTrue()
        {
            // Arrange
            var context = GetDbContext();
            var repo = new UserRepository(context);
            var user = MakeUser();
            await context.Users.AddAsync(user);
            await context.SaveChangesAsync();

            // Act
            var result = await repo.DeleteUserAsync(user.Id);

            // Assert
            Assert.True(result);
            var stored = await context.Users.FindAsync(user.Id);
            Assert.True(stored!.IsDeleted);
        }

        [Fact]
        public async Task DeleteUserAsync_WhenNotFound_ReturnsFalse()
        {
            // Arrange
            var context = GetDbContext();
            var repo = new UserRepository(context);

            // Act
            var result = await repo.DeleteUserAsync(Guid.NewGuid());

            // Assert
            Assert.False(result);
        }
    }
}
