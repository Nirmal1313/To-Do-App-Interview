using Microsoft.EntityFrameworkCore;
using ToDoApp.Domain.Entities.Lists;
using ToDoApp.Infrastructure.Data;
using ToDoApp.Infrastructure.Repositories.Lists;
using Xunit;

namespace ToDoApp.Tests.Infrastructure.ToDoListRepositoryTests
{
    public class ToDoListRepositoryTests
    {
        private AppDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(options);
        }

        [Fact]
        public async Task CreateAsync_ShouldSave_List()
        {
            // Arrange
            var context = GetDbContext();
            var repo = new ToDoListRepository(context);
            var list = new ToDoList { Id = Guid.NewGuid(), Name = "Work", UserId = Guid.NewGuid() };

            // Act
            await repo.CreateAsync(list);

            // Assert
            var result = await context.ToDoLists.FirstOrDefaultAsync();
            Assert.NotNull(result);
            Assert.Equal("Work", result.Name);
        }

        [Fact]
        public async Task GetByIdAsync_WhenExists_ReturnsList()
        {
            // Arrange
            var context = GetDbContext();
            var repo = new ToDoListRepository(context);
            var list = new ToDoList { Id = Guid.NewGuid(), Name = "Groceries", UserId = Guid.NewGuid() };
            await context.ToDoLists.AddAsync(list);
            await context.SaveChangesAsync();

            // Act
            var result = await repo.GetByIdAsync(list.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(list.Id, result!.Id);
        }

        [Fact]
        public async Task GetByIdAsync_WhenNotExists_ReturnsNull()
        {
            // Arrange
            var context = GetDbContext();
            var repo = new ToDoListRepository(context);

            // Act
            var result = await repo.GetByIdAsync(Guid.NewGuid());

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByIdAsync_WhenSoftDeleted_ReturnsNull()
        {
            // Arrange
            var context = GetDbContext();
            var repo = new ToDoListRepository(context);
            var list = new ToDoList { Id = Guid.NewGuid(), Name = "Deleted", UserId = Guid.NewGuid(), IsDeleted = true };
            await context.ToDoLists.AddAsync(list);
            await context.SaveChangesAsync();

            // Act
            var result = await repo.GetByIdAsync(list.Id);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByUserIdAsync_ReturnsOnlyUserLists()
        {
            // Arrange
            var context = GetDbContext();
            var repo = new ToDoListRepository(context);
            var userId = Guid.NewGuid();
            await context.ToDoLists.AddRangeAsync(
                new ToDoList { Id = Guid.NewGuid(), Name = "Mine", UserId = userId },
                new ToDoList { Id = Guid.NewGuid(), Name = "Other", UserId = Guid.NewGuid() }
            );
            await context.SaveChangesAsync();

            // Act
            var result = await repo.GetByUserIdAsync(userId);

            // Assert
            Assert.Single(result);
            Assert.Equal("Mine", result.First().Name);
        }

        [Fact]
        public async Task GetByUserIdAsync_ExcludesSoftDeleted()
        {
            // Arrange
            var context = GetDbContext();
            var repo = new ToDoListRepository(context);
            var userId = Guid.NewGuid();
            await context.ToDoLists.AddRangeAsync(
                new ToDoList { Id = Guid.NewGuid(), Name = "Active", UserId = userId, IsDeleted = false },
                new ToDoList { Id = Guid.NewGuid(), Name = "Gone", UserId = userId, IsDeleted = true }
            );
            await context.SaveChangesAsync();

            // Act
            var result = await repo.GetByUserIdAsync(userId);

            // Assert
            Assert.Single(result);
            Assert.Equal("Active", result.First().Name);
        }

        [Fact]
        public async Task UpdateAsync_WhenFound_UpdatesFields()
        {
            // Arrange
            var context = GetDbContext();
            var repo = new ToDoListRepository(context);
            var list = new ToDoList { Id = Guid.NewGuid(), Name = "Old Name", Color = "#000", UserId = Guid.NewGuid() };
            await context.ToDoLists.AddAsync(list);
            await context.SaveChangesAsync();

            var updated = new ToDoList { Id = list.Id, Name = "New Name", Color = "#fff" };

            // Act
            var result = await repo.UpdateAsync(updated);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("New Name", result!.Name);
            Assert.Equal("#fff", result.Color);
        }

        [Fact]
        public async Task UpdateAsync_WhenNotFound_ReturnsNull()
        {
            // Arrange
            var context = GetDbContext();
            var repo = new ToDoListRepository(context);
            var ghost = new ToDoList { Id = Guid.NewGuid(), Name = "Ghost" };

            // Act
            var result = await repo.UpdateAsync(ghost);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task DeleteAsync_WhenFound_SetsIsDeleted()
        {
            // Arrange
            var context = GetDbContext();
            var repo = new ToDoListRepository(context);
            var list = new ToDoList { Id = Guid.NewGuid(), Name = "To Delete", UserId = Guid.NewGuid() };
            await context.ToDoLists.AddAsync(list);
            await context.SaveChangesAsync();

            var deletedBy = Guid.NewGuid();

            // Act
            var result = await repo.DeleteAsync(list.Id, deletedBy);

            // Assert
            Assert.True(result);
            var stored = await context.ToDoLists.FindAsync(list.Id);
            Assert.True(stored!.IsDeleted);
            Assert.Equal(deletedBy, stored.UpdatedBy);
            Assert.NotNull(stored.LastUpdatedDate);
        }

        [Fact]
        public async Task DeleteAsync_WhenNotFound_ReturnsFalse()
        {
            // Arrange
            var context = GetDbContext();
            var repo = new ToDoListRepository(context);

            // Act
            var result = await repo.DeleteAsync(Guid.NewGuid(), Guid.NewGuid());

            // Assert
            Assert.False(result);
        }
    }
}
