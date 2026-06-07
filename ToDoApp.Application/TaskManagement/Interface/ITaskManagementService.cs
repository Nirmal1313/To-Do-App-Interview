using ToDoApp.Application.DTOs.TaskDto;
using ToDoApp.Domain.Common.Enums;
using TaskStatus = ToDoApp.Domain.Common.Enums.TaskStatus;

namespace ToDoApp.Application.TaskManagement.Interface
{
    public interface ITaskManagementService
    {
        Task<ToDoTaskDto> AddTaskAsync(ToDoTaskDto dto, Guid userId);
        Task<ToDoTaskDto?> GetTaskByIdAsync(Guid id, Guid userId);
        Task<ToDoTaskDto?> UpdateTaskAsync(Guid id, ToDoTaskDto dto, Guid userId);
        Task<bool> DeleteTaskAsync(Guid id, Guid userId);
        Task<List<ToDoTaskDto>> GetTasksByStatusAsync(Guid userId, TaskStatus status);
        Task<List<ToDoTaskDto>> GetTasksByPriorityAsync(Guid userId, TaskPriority status);
        Task<List<ToDoTaskDto>> GetAllAsync(Guid userId);
        Task<List<ToDoTaskDto?>> GetTaskByTitleAsync(string title, Guid userId);
        Task<List<ToDoTaskDto>> GetTasksByListIdAsync(Guid listId, Guid userId);
        Task ReplaceTaskTagsAsync(Guid taskId, List<Guid> tagIds, Guid userId);
    }
}
