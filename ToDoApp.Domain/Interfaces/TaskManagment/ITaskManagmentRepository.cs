using ToDoApp.Domain.Common.Enums;
using ToDoApp.Domain.Entities.ToDoTask.TaskDetails;
using TaskStatus = ToDoApp.Domain.Common.Enums.TaskStatus;

namespace ToDoApp.Domain.Interfaces.TaskManagment
{
    public interface ITaskManagmentRepository
    {
        Task<ToDoTasks> CreateTaskAsync(ToDoTasks taskDetails);
        Task<ToDoTasks?> DeleteTaskAsync(Guid id, Guid deletedBy);
        Task<ToDoTasks?> UpdateTaskAsync(ToDoTasks taskDetails);
        Task<ToDoTasks?> GetTaskByIdAsync(Guid id);
        Task<List<ToDoTasks?>?> GetTaskByNameAsync(string name);
        Task<IEnumerable<ToDoTasks>> GetTasksByStatusAsync(TaskStatus status, Guid userId);
        Task<IEnumerable<ToDoTasks>> GetTasksByPriorityAsync(TaskPriority priority, Guid userId);
        Task<IEnumerable<ToDoTasks>> GetTasksByListIdAsync(Guid listId);
        Task<IEnumerable<ToDoTasks>> GetTasksByUserIdAsync(Guid userId);
        Task<int> DeleteByListIdAsync(Guid listId, Guid deletedBy);
    }
}
