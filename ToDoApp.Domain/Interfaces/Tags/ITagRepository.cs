using ToDoApp.Domain.Entities.Tags;

namespace ToDoApp.Domain.Interfaces.Tags
{
    public interface ITagRepository
    {
        Task<Tag> CreateAsync(Tag tag);
        Task<Tag?> GetByIdAsync(Guid id);
        Task<IEnumerable<Tag>> GetByUserIdAsync(Guid userId);
        Task<bool> DeleteAsync(Guid id, Guid deletedBy);
        Task<bool> AssignTagToTaskAsync(Guid tagId, Guid taskId);
        Task<bool> RemoveTagFromTaskAsync(Guid tagId, Guid taskId);
        Task<IEnumerable<TaskTag>> GetTaskTagsAsync(Guid taskId);
        Task ReplaceTaskTagsAsync(Guid taskId, IEnumerable<Guid> tagIds);
    }
}
