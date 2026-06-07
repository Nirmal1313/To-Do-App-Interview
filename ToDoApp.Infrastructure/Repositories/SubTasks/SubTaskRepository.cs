using Microsoft.EntityFrameworkCore;
using ToDoApp.Domain.Entities.SubTasks;
using ToDoApp.Domain.Interfaces.SubTasks;
using ToDoApp.Infrastructure.Data;

namespace ToDoApp.Infrastructure.Repositories.SubTasks
{
    public class SubTaskRepository : ISubTaskRepository
    {
        private readonly AppDbContext _dbContext;

        public SubTaskRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<SubTask> CreateAsync(SubTask subTask)
        {
            await _dbContext.SubTasks.AddAsync(subTask);
            await _dbContext.SaveChangesAsync();
            return subTask;
        }

        public async Task<SubTask?> GetByIdAsync(Guid id)
        {
            return await _dbContext.SubTasks.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        }

        public async Task<IEnumerable<SubTask>> GetByParentTaskIdAsync(Guid parentTaskId)
        {
            return await _dbContext.SubTasks.AsNoTracking().Where(x => x.ParentTaskId == parentTaskId && !x.IsDeleted).ToListAsync();
        }

        public async Task<SubTask?> UpdateAsync(SubTask subTask)
        {
            var existing = await _dbContext.SubTasks.FirstOrDefaultAsync(x => x.Id == subTask.Id && !x.IsDeleted);

            if (existing == null)
                return null;

            existing.Title = subTask.Title;
            existing.IsCompleted = subTask.IsCompleted;

            await _dbContext.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteAsync(Guid id, Guid deletedBy)
        {
            var subTask = await _dbContext.SubTasks.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

            if (subTask == null)
                return false;

            subTask.IsDeleted = true;
            subTask.UpdatedBy = deletedBy;
            subTask.LastUpdatedDate = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();
            return true;
        }
    }
}
