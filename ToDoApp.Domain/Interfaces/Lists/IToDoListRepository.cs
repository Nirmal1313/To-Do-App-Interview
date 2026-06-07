using ToDoApp.Domain.Entities.Lists;

namespace ToDoApp.Domain.Interfaces.Lists
{
    public interface IToDoListRepository
    {
        Task<ToDoList> CreateAsync(ToDoList list);
        Task<ToDoList?> GetByIdAsync(Guid id);
        Task<IEnumerable<ToDoList>> GetByUserIdAsync(Guid userId);
        Task<ToDoList?> UpdateAsync(ToDoList list);
        Task<bool> DeleteAsync(Guid id, Guid deletedBy);
    }
}
