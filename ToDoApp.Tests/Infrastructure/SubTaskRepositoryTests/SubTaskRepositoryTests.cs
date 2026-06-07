using Microsoft.EntityFrameworkCore;
using ToDoApp.Domain.Entities.SubTasks;
using ToDoApp.Infrastructure.Data;
using ToDoApp.Infrastructure.Repositories.SubTasks;
using Xunit;

namespace ToDoApp.Tests.Infrastructure.SubTaskRepositoryTests
{
    public class SubTaskRepositoryTests
    {
        private AppDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(options);
        }

        [Fact]
        public async Task CreateAsync_ShouldSave_SubTask()
        {
            // Arrange
            var context = GetDbContext();
            var repo = new SubTaskRepository(context);
            var subTask = new SubTask { Id = Guid.NewGuid(), Title = "Step 1", ParentTaskId = Guid.NewGuid() };

            // Act
            await repo.CreateAsync(subTask);

            // Assert
            var result = await context.SubTasks.FirstOrDefaultAsync();
            Assert.NotNull(result);
            Assert.Equal("Step 1", result.Title);
        }

        [Fact]
        public async Task GetByIdAsync_WhenExists_ReturnsSubTask()
        {
            // Arrange
            var context = GetDbContext();
            var repo = new SubTaskRepository(context);
            var subTask = new SubTask { Id = Guid.NewGuid(), Title = "Find Me", ParentTaskId = Guid.NewGuid() };
            await context.SubTasks.AddAsync(subTask);
            await context.SaveChangesAsync();

            // Act
            var result = await repo.GetByIdAsync(subTask.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(subTask.Id, result!.Id);
        }

        [Fact]
        public async Task GetByIdAsync_WhenNotExists_ReturnsNull()
        {
            // Arrange
            var context = GetDbContext();
            var repo = new SubTaskRepository(context);

            // Act
            var result = await repo.GetByIdAsync(Guid.NewGuid());

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByParentTaskIdAsync_ReturnsSubTasksForParent()
        {
            // Arrange
            var context = GetDbContext();
            var repo = new SubTaskRepository(context);
            var parentId = Guid.NewGuid();
            await context.SubTasks.AddRangeAsync(
                new SubTask { Id = Guid.NewGuid(), Title = "Child 1", ParentTaskId = parentId },
                new SubTask { Id = Guid.NewGuid(), Title = "Child 2", ParentTaskId = parentId },
                new SubTask { Id = Guid.NewGuid(), Title = "Other", ParentTaskId = Guid.NewGuid() }
            );
            await context.SaveChangesAsync();

            // Act
            var result = await repo.GetByParentTaskIdAsync(parentId);

            // Assert
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task UpdateAsync_WhenFound_UpdatesFields()
        {
            // Arrange
            var context = GetDbContext();
            var repo = new SubTaskRepository(context);
            var subTask = new SubTask { Id = Guid.NewGuid(), Title = "Old", ParentTaskId = Guid.NewGuid() };
            await context.SubTasks.AddAsync(subTask);
            await context.SaveChangesAsync();

            var updated = new SubTask { Id = subTask.Id, Title = "New", IsCompleted = true, ParentTaskId = subTask.ParentTaskId };

            // Act
            var result = await repo.UpdateAsync(updated);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("New", result!.Title);
            Assert.True(result.IsCompleted);
        }

        [Fact]
        public async Task UpdateAsync_WhenNotFound_ReturnsNull()
        {
            // Arrange
            var context = GetDbContext();
            var repo = new SubTaskRepository(context);
            var ghost = new SubTask { Id = Guid.NewGuid(), Title = "Ghost", ParentTaskId = Guid.NewGuid() };

            // Act
            var result = await repo.UpdateAsync(ghost);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task DeleteAsync_WhenFound_SetsIsDeletedAndAuditFields()
        {
            // Arrange
            var context = GetDbContext();
            var repo = new SubTaskRepository(context);
            var subTask = new SubTask { Id = Guid.NewGuid(), Title = "Delete Me", ParentTaskId = Guid.NewGuid() };
            await context.SubTasks.AddAsync(subTask);
            await context.SaveChangesAsync();

            var deletedBy = Guid.NewGuid();

            // Act
            var result = await repo.DeleteAsync(subTask.Id, deletedBy);

            // Assert
            Assert.True(result);
            var stored = await context.SubTasks.FindAsync(subTask.Id);
            Assert.True(stored!.IsDeleted);
            Assert.Equal(deletedBy, stored.UpdatedBy);
            Assert.NotNull(stored.LastUpdatedDate);
        }

        [Fact]
        public async Task DeleteAsync_WhenNotFound_ReturnsFalse()
        {
            // Arrange
            var context = GetDbContext();
            var repo = new SubTaskRepository(context);

            // Act
            var result = await repo.DeleteAsync(Guid.NewGuid(), Guid.NewGuid());

            // Assert
            Assert.False(result);
        }
    }
}
