using Microsoft.EntityFrameworkCore;
using ToDoApp.Domain.Common.Enums;
using ToDoApp.Domain.Entities.ToDoTask.TaskDetails;
using ToDoApp.Domain.Interfaces.TaskManagment;
using ToDoApp.Infrastructure.Data;
using TaskStatus = ToDoApp.Domain.Common.Enums.TaskStatus;

namespace ToDoApp.Infrastructure.Repositories.TaskManagement
{
    public class TaskManagementRepository : ITaskManagmentRepository
    {
        private readonly AppDbContext _dbContext;

        public TaskManagementRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ToDoTasks> CreateTaskAsync(ToDoTasks task)
        {
            await _dbContext.ToDoTasks.AddAsync(task);
            await _dbContext.SaveChangesAsync();
            return task;
        }

        public async Task<ToDoTasks?> GetTaskByIdAsync(Guid id)
        {
            return await _dbContext.ToDoTasks.AsNoTracking().Include(x => x.TaskTags).ThenInclude(tt => tt.Tag).Include(x => x.SubTasks).FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        }

        public async Task<List<ToDoTasks?>?> GetTaskByNameAsync(string name)
        {
            return await _dbContext.ToDoTasks.AsNoTracking().Include(x => x.TaskTags).ThenInclude(tt => tt.Tag).Include(x => x.SubTasks).Where(x => x.Title == name && !x.IsDeleted).ToListAsync<ToDoTasks?>();
        }

        public async Task<IEnumerable<ToDoTasks>> GetTasksByPriorityAsync(TaskPriority priority, Guid userId)
        {
            return await _dbContext.ToDoTasks.AsNoTracking().Include(x => x.TaskTags).ThenInclude(tt => tt.Tag).Include(x => x.SubTasks).Where(x => x.Priority == priority && x.UserId == userId && !x.IsDeleted).ToListAsync();
        }

        public async Task<IEnumerable<ToDoTasks>> GetTasksByStatusAsync(TaskStatus status, Guid userId)
        {
            return await _dbContext.ToDoTasks.AsNoTracking().Include(x => x.TaskTags).ThenInclude(tt => tt.Tag).Include(x => x.SubTasks).Where(x => x.Status == status && x.UserId == userId && !x.IsDeleted).ToListAsync();
        }

        public async Task<IEnumerable<ToDoTasks>> GetTasksByListIdAsync(Guid listId)
        {
            return await _dbContext.ToDoTasks.AsNoTracking().Include(x => x.TaskTags).ThenInclude(tt => tt.Tag).Include(x => x.SubTasks).Where(x => x.ListId == listId && !x.IsDeleted).ToListAsync();
        }

        public async Task<IEnumerable<ToDoTasks>> GetTasksByUserIdAsync(Guid userId)
        {
            return await _dbContext.ToDoTasks.AsNoTracking().Include(x => x.TaskTags).ThenInclude(tt => tt.Tag).Include(x => x.SubTasks).Where(x => x.UserId == userId && !x.IsDeleted).ToListAsync();
        }

        public async Task<ToDoTasks?> UpdateTaskAsync(ToDoTasks task)
        {
            var existingTask = await _dbContext.ToDoTasks.FirstOrDefaultAsync(x => x.Id == task.Id && !x.IsDeleted);

            if (existingTask == null)
                return null;

            existingTask.Title = task.Title;
            existingTask.Description = task.Description;
            existingTask.Priority = task.Priority;
            existingTask.Status = task.Status;
            existingTask.ListId = task.ListId;
            existingTask.UpdatedBy = task.UpdatedBy;
            existingTask.LastUpdatedDate = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            return existingTask;
        }

        public async Task<ToDoTasks?> DeleteTaskAsync(Guid id, Guid deletedBy)
        {
            var task = await _dbContext.ToDoTasks.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

            if (task == null)
                return null;

            task.IsDeleted = true;
            task.UpdatedBy = deletedBy;
            task.LastUpdatedDate = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            return task;
        }

        public async Task<int> DeleteByListIdAsync(Guid listId, Guid deletedBy)
        {
            var tasks = await _dbContext.ToDoTasks
                .Where(x => x.ListId == listId && !x.IsDeleted)
                .ToListAsync();

            foreach (var task in tasks)
            {
                task.IsDeleted = true;
                task.UpdatedBy = deletedBy;
                task.LastUpdatedDate = DateTime.UtcNow;
            }

            await _dbContext.SaveChangesAsync();
            return tasks.Count;
        }
    }
}
