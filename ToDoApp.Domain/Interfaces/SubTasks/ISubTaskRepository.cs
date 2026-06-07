using ToDoApp.Domain.Entities.SubTasks;

namespace ToDoApp.Domain.Interfaces.SubTasks
{
    public interface ISubTaskRepository
    {
        Task<SubTask> CreateAsync(SubTask subTask);
        Task<SubTask?> GetByIdAsync(Guid id);
        Task<IEnumerable<SubTask>> GetByParentTaskIdAsync(Guid parentTaskId);
        Task<SubTask?> UpdateAsync(SubTask subTask);
        Task<bool> DeleteAsync(Guid id, Guid deletedBy);
    }
}
