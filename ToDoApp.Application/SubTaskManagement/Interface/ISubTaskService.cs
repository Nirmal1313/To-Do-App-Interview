using ToDoApp.Application.DTOs.SubTaskDto;

namespace ToDoApp.Application.SubTaskManagement.Interface
{
    public interface ISubTaskService
    {
        Task<SubTaskDto> CreateAsync(SubTaskDto dto);
        Task<IEnumerable<SubTaskDto>> GetByTaskIdAsync(Guid parentTaskId);
        Task<SubTaskDto?> UpdateAsync(Guid id, SubTaskDto dto);
        Task<bool> DeleteAsync(Guid id, Guid userId);
    }
}
