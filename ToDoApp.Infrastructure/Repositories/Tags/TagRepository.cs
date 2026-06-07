using Microsoft.EntityFrameworkCore;
using ToDoApp.Domain.Entities.Tags;
using ToDoApp.Domain.Interfaces.Tags;
using ToDoApp.Infrastructure.Data;

namespace ToDoApp.Infrastructure.Repositories.Tags
{
    public class TagRepository : ITagRepository
    {
        private readonly AppDbContext _dbContext;

        public TagRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Tag> CreateAsync(Tag tag)
        {
            await _dbContext.Tags.AddAsync(tag);
            await _dbContext.SaveChangesAsync();
            return tag;
        }

        public async Task<Tag?> GetByIdAsync(Guid id)
        {
            return await _dbContext.Tags.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        }

        public async Task<IEnumerable<Tag>> GetByUserIdAsync(Guid userId)
        {
            return await _dbContext.Tags.AsNoTracking().Where(x => x.UserId == userId && !x.IsDeleted).ToListAsync();
        }

        public async Task<IEnumerable<TaskTag>> GetTaskTagsAsync(Guid taskId)
        {
            return await _dbContext.TaskTags.AsNoTracking().Include(tt => tt.Tag).Where(tt => tt.TaskId == taskId && !tt.IsDeleted).ToListAsync();
        }

        public async Task<bool> AssignTagToTaskAsync(Guid tagId, Guid taskId)
        {
            var existing = await _dbContext.TaskTags.FirstOrDefaultAsync(tt => tt.TagId == tagId && tt.TaskId == taskId);
            if (existing != null)
            {
                if (!existing.IsDeleted)
                    return true;
                existing.IsDeleted = false;
                existing.LastUpdatedDate = DateTime.UtcNow;
            }
            else
            {
                await _dbContext.TaskTags.AddAsync(new TaskTag { TagId = tagId, TaskId = taskId });
            }
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveTagFromTaskAsync(Guid tagId, Guid taskId)
        {
            var taskTag = await _dbContext.TaskTags.FirstOrDefaultAsync(tt => tt.TagId == tagId && tt.TaskId == taskId && !tt.IsDeleted);
            if (taskTag == null)
                return false;

            taskTag.IsDeleted = true;
            taskTag.LastUpdatedDate = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task ReplaceTaskTagsAsync(Guid taskId, IEnumerable<Guid> tagIds)
        {
            var newIds = tagIds.ToHashSet();
            var existing = await _dbContext.TaskTags.Where(tt => tt.TaskId == taskId).ToListAsync();
            var existingByTag = existing.ToDictionary(tt => tt.TagId);

            foreach (var tt in existing.Where(tt => !newIds.Contains(tt.TagId) && !tt.IsDeleted))
            {
                tt.IsDeleted = true;
                tt.LastUpdatedDate = DateTime.UtcNow;
            }

            foreach (var id in newIds)
            {
                if (existingByTag.TryGetValue(id, out var tt))
                    tt.IsDeleted = false;
                else
                    await _dbContext.TaskTags.AddAsync(new TaskTag { TaskId = taskId, TagId = id });
            }

            await _dbContext.SaveChangesAsync();
        }

        public async Task<bool> DeleteAsync(Guid id, Guid deletedBy)
        {
            var tag = await _dbContext.Tags.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
            if (tag == null)
                return false;

            tag.IsDeleted = true;
            tag.UpdatedBy = deletedBy;
            tag.LastUpdatedDate = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();
            return true;
        }
    }
}
