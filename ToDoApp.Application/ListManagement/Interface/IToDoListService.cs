using ToDoApp.Application.DTOs.ListDto;

namespace ToDoApp.Application.ListManagement.Interface
{
    public interface IToDoListService
    {
        Task<ToDoListDto> CreateAsync(ToDoListDto dto, Guid userId);
        Task<ToDoListDto?> GetByIdAsync(Guid id, Guid userId);
        Task<IEnumerable<ToDoListDto>> GetAllAsync(Guid userId);
        Task<ToDoListDto?> UpdateAsync(Guid id, ToDoListDto dto, Guid userId);
        Task<bool> DeleteAsync(Guid id, Guid userId);
    }
}
