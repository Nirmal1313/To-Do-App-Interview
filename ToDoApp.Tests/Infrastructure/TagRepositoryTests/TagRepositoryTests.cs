using Microsoft.EntityFrameworkCore;
using ToDoApp.Domain.Entities.Tags;
using ToDoApp.Infrastructure.Data;
using ToDoApp.Infrastructure.Repositories.Tags;
using Xunit;

namespace ToDoApp.Tests.Infrastructure.TagRepositoryTests
{
    public class TagRepositoryTests
    {
        private AppDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(options);
        }

        [Fact]
        public async Task CreateAsync_ShouldSave_Tag()
        {
            // Arrange
            var context = GetDbContext();
            var repo = new TagRepository(context);
            var tag = new Tag { Id = Guid.NewGuid(), Name = "Bug", UserId = Guid.NewGuid() };

            // Act
            await repo.CreateAsync(tag);

            // Assert
            var result = await context.Tags.FirstOrDefaultAsync();
            Assert.NotNull(result);
            Assert.Equal("Bug", result.Name);
        }

        [Fact]
        public async Task GetByIdAsync_WhenExists_ReturnsTag()
        {
            // Arrange
            var context = GetDbContext();
            var repo = new TagRepository(context);
            var tag = new Tag { Id = Guid.NewGuid(), Name = "Feature", UserId = Guid.NewGuid() };
            await context.Tags.AddAsync(tag);
            await context.SaveChangesAsync();

            // Act
            var result = await repo.GetByIdAsync(tag.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(tag.Id, result!.Id);
        }

        [Fact]
        public async Task GetByIdAsync_WhenNotExists_ReturnsNull()
        {
            // Arrange
            var context = GetDbContext();
            var repo = new TagRepository(context);

            // Act
            var result = await repo.GetByIdAsync(Guid.NewGuid());

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByUserIdAsync_ReturnsOnlyUserTags()
        {
            // Arrange
            var context = GetDbContext();
            var repo = new TagRepository(context);
            var userId = Guid.NewGuid();
            await context.Tags.AddRangeAsync(
                new Tag { Id = Guid.NewGuid(), Name = "Mine", UserId = userId },
                new Tag { Id = Guid.NewGuid(), Name = "Other", UserId = Guid.NewGuid() }
            );
            await context.SaveChangesAsync();

            // Act
            var result = await repo.GetByUserIdAsync(userId);

            // Assert
            Assert.Single(result);
            Assert.Equal("Mine", result.First().Name);
        }

        [Fact]
        public async Task AssignTagToTaskAsync_CreatesTaskTag()
        {
            // Arrange
            var context = GetDbContext();
            var repo = new TagRepository(context);
            var tagId = Guid.NewGuid();
            var taskId = Guid.NewGuid();

            // Act
            var result = await repo.AssignTagToTaskAsync(tagId, taskId);

            // Assert
            Assert.True(result);
            Assert.Single(context.TaskTags);
        }

        [Fact]
        public async Task AssignTagToTaskAsync_WhenAlreadyExists_DoesNotDuplicate()
        {
            // Arrange
            var context = GetDbContext();
            var repo = new TagRepository(context);
            var tagId = Guid.NewGuid();
            var taskId = Guid.NewGuid();
            await context.TaskTags.AddAsync(new TaskTag { TagId = tagId, TaskId = taskId });
            await context.SaveChangesAsync();

            // Act
            var result = await repo.AssignTagToTaskAsync(tagId, taskId);

            // Assert
            Assert.True(result);
            Assert.Single(context.TaskTags);
        }

        [Fact]
        public async Task RemoveTagFromTaskAsync_WhenExists_RemovesTaskTag()
        {
            // Arrange
            var context = GetDbContext();
            var repo = new TagRepository(context);
            var tagId = Guid.NewGuid();
            var taskId = Guid.NewGuid();
            await context.TaskTags.AddAsync(new TaskTag { TagId = tagId, TaskId = taskId });
            await context.SaveChangesAsync();

            // Act
            var result = await repo.RemoveTagFromTaskAsync(tagId, taskId);

            // Assert
            Assert.True(result);
            var stored = await context.TaskTags.FindAsync(taskId, tagId);
            Assert.True(stored!.IsDeleted);
        }

        [Fact]
        public async Task RemoveTagFromTaskAsync_WhenNotExists_ReturnsFalse()
        {
            // Arrange
            var context = GetDbContext();
            var repo = new TagRepository(context);

            // Act
            var result = await repo.RemoveTagFromTaskAsync(Guid.NewGuid(), Guid.NewGuid());

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task GetTaskTagsAsync_ReturnsTagsForTask()
        {
            // Arrange
            var context = GetDbContext();
            var repo = new TagRepository(context);
            var tagId = Guid.NewGuid();
            var taskId = Guid.NewGuid();
            await context.Tags.AddAsync(new Tag { Id = tagId, Name = "Sprint", UserId = Guid.NewGuid() });
            await context.TaskTags.AddAsync(new TaskTag { TagId = tagId, TaskId = taskId });
            await context.SaveChangesAsync();

            // Act
            var result = await repo.GetTaskTagsAsync(taskId);

            // Assert
            Assert.Single(result);
            Assert.Equal(tagId, result.First().TagId);
        }

        [Fact]
        public async Task DeleteAsync_WhenFound_SetsIsDeletedAndAuditFields()
        {
            // Arrange
            var context = GetDbContext();
            var repo = new TagRepository(context);
            var tag = new Tag { Id = Guid.NewGuid(), Name = "Remove Me", UserId = Guid.NewGuid() };
            await context.Tags.AddAsync(tag);
            await context.SaveChangesAsync();

            var deletedBy = Guid.NewGuid();

            // Act
            var result = await repo.DeleteAsync(tag.Id, deletedBy);

            // Assert
            Assert.True(result);
            var stored = await context.Tags.FindAsync(tag.Id);
            Assert.True(stored!.IsDeleted);
            Assert.Equal(deletedBy, stored.UpdatedBy);
            Assert.NotNull(stored.LastUpdatedDate);
        }

        [Fact]
        public async Task DeleteAsync_WhenNotFound_ReturnsFalse()
        {
            // Arrange
            var context = GetDbContext();
            var repo = new TagRepository(context);

            // Act
            var result = await repo.DeleteAsync(Guid.NewGuid(), Guid.NewGuid());

            // Assert
            Assert.False(result);
        }
    }
}
