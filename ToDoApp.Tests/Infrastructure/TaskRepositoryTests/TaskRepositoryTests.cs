using Microsoft.EntityFrameworkCore;
using ToDoApp.Domain.Common.Enums;
using ToDoApp.Domain.Entities.ToDoTask.TaskDetails;
using ToDoApp.Infrastructure.Data;
using ToDoApp.Infrastructure.Repositories.TaskManagement;
using Xunit;
using DomainTaskStatus = ToDoApp.Domain.Common.Enums.TaskStatus;

namespace ToDoApp.Tests.Infrastructure.TaskRepositoryTests
{
    public class TaskRepositoryTests
    {
        private AppDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new AppDbContext(options);
        }

        [Fact]
        public async Task CreateTask_ShouldSave_Task()
        {
            // Arrange
            var context = GetDbContext();
            var repo = new TaskManagementRepository(context);
            var task = new ToDoTasks { Id = Guid.NewGuid(), Title = "Test Task", Description = "desc" };

            // Act
            await repo.CreateTaskAsync(task);

            // Assert
            var result = await context.ToDoTasks.FirstOrDefaultAsync();
            Assert.NotNull(result);
            Assert.Equal("Test Task", result.Title);
        }

        [Fact]
        public async Task GetTaskByIdAsync_WhenExists_ReturnsTask()
        {
            // Arrange
            var context = GetDbContext();
            var repo = new TaskManagementRepository(context);
            var task = new ToDoTasks { Id = Guid.NewGuid(), Title = "Find Me", Description = "desc" };
            await context.ToDoTasks.AddAsync(task);
            await context.SaveChangesAsync();

            // Act
            var result = await repo.GetTaskByIdAsync(task.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(task.Id, result!.Id);
        }

        [Fact]
        public async Task GetTaskByIdAsync_WhenNotExists_ReturnsNull()
        {
            // Arrange
            var context = GetDbContext();
            var repo = new TaskManagementRepository(context);

            // Act
            var result = await repo.GetTaskByIdAsync(Guid.NewGuid());

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetTaskByIdAsync_WhenSoftDeleted_ReturnsNull()
        {
            // Arrange
            var context = GetDbContext();
            var repo = new TaskManagementRepository(context);
            var task = new ToDoTasks { Id = Guid.NewGuid(), Title = "Deleted", Description = "desc", IsDeleted = true };
            await context.ToDoTasks.AddAsync(task);
            await context.SaveChangesAsync();

            // Act
            var result = await repo.GetTaskByIdAsync(task.Id);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetTasksByUserIdAsync_ReturnsOnlyUserNonDeletedTasks()
        {
            // Arrange
            var context = GetDbContext();
            var repo = new TaskManagementRepository(context);
            var userId = Guid.NewGuid();
            await context.ToDoTasks.AddRangeAsync(
                new ToDoTasks { Id = Guid.NewGuid(), Title = "Mine", Description = "d", UserId = userId, IsDeleted = false },
                new ToDoTasks { Id = Guid.NewGuid(), Title = "Gone", Description = "d", UserId = userId, IsDeleted = true },
                new ToDoTasks { Id = Guid.NewGuid(), Title = "Other", Description = "d", UserId = Guid.NewGuid(), IsDeleted = false }
            );
            await context.SaveChangesAsync();

            // Act
            var result = await repo.GetTasksByUserIdAsync(userId);

            // Assert
            Assert.Single(result);
            Assert.Equal("Mine", result.First().Title);
        }

        [Fact]
        public async Task GetTaskByNameAsync_WhenExists_ReturnsTasks()
        {
            // Arrange
            var context = GetDbContext();
            var repo = new TaskManagementRepository(context);
            var task = new ToDoTasks { Id = Guid.NewGuid(), Title = "Sprint Task", Description = "d" };
            await context.ToDoTasks.AddAsync(task);
            await context.SaveChangesAsync();

            // Act
            var result = await repo.GetTaskByNameAsync("Sprint Task");

            // Assert
            Assert.NotNull(result);
            Assert.Single(result!);
        }

        [Fact]
        public async Task GetTaskByNameAsync_WhenNotExists_ReturnsEmpty()
        {
            // Arrange
            var context = GetDbContext();
            var repo = new TaskManagementRepository(context);

            // Act
            var result = await repo.GetTaskByNameAsync("Nothing");

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result!);
        }

        [Fact]
        public async Task GetTasksByStatusAsync_ReturnsMatchingNonDeletedTasksForUser()
        {
            // Arrange
            var context = GetDbContext();
            var repo = new TaskManagementRepository(context);
            var userId = Guid.NewGuid();
            await context.ToDoTasks.AddRangeAsync(
                new ToDoTasks { Id = Guid.NewGuid(), Title = "T1", Description = "d", Status = DomainTaskStatus.Created, UserId = userId, IsDeleted = false },
                new ToDoTasks { Id = Guid.NewGuid(), Title = "T2", Description = "d", Status = DomainTaskStatus.InProgress, UserId = userId, IsDeleted = false },
                new ToDoTasks { Id = Guid.NewGuid(), Title = "T3", Description = "d", Status = DomainTaskStatus.Created, UserId = userId, IsDeleted = true },
                new ToDoTasks { Id = Guid.NewGuid(), Title = "T4", Description = "d", Status = DomainTaskStatus.Created, UserId = Guid.NewGuid(), IsDeleted = false }
            );
            await context.SaveChangesAsync();

            // Act
            var result = await repo.GetTasksByStatusAsync(DomainTaskStatus.Created, userId);

            // Assert
            Assert.Single(result);
            Assert.Equal("T1", result.First().Title);
        }

        [Fact]
        public async Task GetTasksByPriorityAsync_ReturnsMatchingNonDeletedTasksForUser()
        {
            // Arrange
            var context = GetDbContext();
            var repo = new TaskManagementRepository(context);
            var userId = Guid.NewGuid();
            await context.ToDoTasks.AddRangeAsync(
                new ToDoTasks { Id = Guid.NewGuid(), Title = "High", Description = "d", Priority = TaskPriority.High, UserId = userId, IsDeleted = false },
                new ToDoTasks { Id = Guid.NewGuid(), Title = "Low", Description = "d", Priority = TaskPriority.Low, UserId = userId, IsDeleted = false },
                new ToDoTasks { Id = Guid.NewGuid(), Title = "HighDel", Description = "d", Priority = TaskPriority.High, UserId = userId, IsDeleted = true },
                new ToDoTasks { Id = Guid.NewGuid(), Title = "OtherHigh", Description = "d", Priority = TaskPriority.High, UserId = Guid.NewGuid(), IsDeleted = false }
            );
            await context.SaveChangesAsync();

            // Act
            var result = await repo.GetTasksByPriorityAsync(TaskPriority.High, userId);

            // Assert
            Assert.Single(result);
            Assert.Equal("High", result.First().Title);
        }

        [Fact]
        public async Task UpdateTaskAsync_WhenFound_UpdatesFields()
        {
            // Arrange
            var context = GetDbContext();
            var repo = new TaskManagementRepository(context);
            var task = new ToDoTasks { Id = Guid.NewGuid(), Title = "Old Title", Description = "d" };
            await context.ToDoTasks.AddAsync(task);
            await context.SaveChangesAsync();

            task.Title = "New Title";
            task.Description = "Updated";

            // Act
            var result = await repo.UpdateTaskAsync(task);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("New Title", result!.Title);
            Assert.Equal("Updated", result.Description);
        }

        [Fact]
        public async Task UpdateTaskAsync_WhenNotFound_ReturnsNull()
        {
            // Arrange
            var context = GetDbContext();
            var repo = new TaskManagementRepository(context);
            var ghost = new ToDoTasks { Id = Guid.NewGuid(), Title = "Ghost", Description = "d" };

            // Act
            var result = await repo.UpdateTaskAsync(ghost);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task DeleteTaskAsync_SetsIsDeletedTrue()
        {
            // Arrange
            var context = GetDbContext();
            var repo = new TaskManagementRepository(context);
            var task = new ToDoTasks { Id = Guid.NewGuid(), Title = "To Delete", Description = "d" };
            await context.ToDoTasks.AddAsync(task);
            await context.SaveChangesAsync();
            var deletedBy = Guid.NewGuid();

            // Act
            await repo.DeleteTaskAsync(task.Id, deletedBy);

            // Assert
            var stored = await context.ToDoTasks.FindAsync(task.Id);
            Assert.True(stored!.IsDeleted);
            Assert.Equal(deletedBy, stored.UpdatedBy);
            Assert.NotNull(stored.LastUpdatedDate);
        }

        [Fact]
        public async Task DeleteTaskAsync_WhenNotFound_ReturnsNull()
        {
            // Arrange
            var context = GetDbContext();
            var repo = new TaskManagementRepository(context);

            // Act
            var result = await repo.DeleteTaskAsync(Guid.NewGuid(), Guid.NewGuid());

            // Assert
            Assert.Null(result);
        }
    }
}