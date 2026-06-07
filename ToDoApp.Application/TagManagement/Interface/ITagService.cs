using ToDoApp.Application.DTOs.TagDto;

namespace ToDoApp.Application.TagManagement.Interface
{
    public interface ITagService
    {
        Task<TagDto> CreateAsync(TagDto dto, Guid userId);
        Task<IEnumerable<TagDto>> GetAllAsync(Guid userId);
        Task<bool> DeleteAsync(Guid id, Guid userId);
        Task<bool> AssignTagToTaskAsync(Guid tagId, Guid taskId, Guid userId);
        Task<bool> RemoveTagFromTaskAsync(Guid tagId, Guid taskId, Guid userId);
    }
}
