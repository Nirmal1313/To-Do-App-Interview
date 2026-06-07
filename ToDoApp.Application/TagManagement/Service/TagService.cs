using AutoMapper;
using Microsoft.Extensions.Logging;
using ToDoApp.Application.DTOs.TagDto;
using ToDoApp.Application.TagManagement.Interface;
using ToDoApp.Domain.Entities.Tags;
using ToDoApp.Domain.Interfaces.Tags;

namespace ToDoApp.Application.TagManagement.Service
{
    public class TagService : ITagService
    {
        private readonly ITagRepository _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<TagService> _logger;

        public TagService(ITagRepository repository, IMapper mapper, ILogger<TagService> logger)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<TagDto> CreateAsync(TagDto dto, Guid userId)
        {
            _logger.LogInformation("Creating tag for user {UserId}", userId);
            var entity = _mapper.Map<Tag>(dto);
            entity.UserId = userId;
            var result = await _repository.CreateAsync(entity);
            _logger.LogInformation("Tag created. TagId: {TagId}", result.Id);
            return _mapper.Map<TagDto>(result);
        }

        public async Task<IEnumerable<TagDto>> GetAllAsync(Guid userId)
        {
            var entities = await _repository.GetByUserIdAsync(userId);
            return _mapper.Map<IEnumerable<TagDto>>(entities);
        }

        public async Task<bool> DeleteAsync(Guid id, Guid userId)
        {
            var tag = await _repository.GetByIdAsync(id);
            if (tag == null || tag.UserId != userId)
            {
                _logger.LogWarning("Delete failed. Tag not found. TagId: {TagId}", id);
                throw new KeyNotFoundException("Tag not found.");
            }

            _logger.LogInformation("Deleting tag {TagId}", id);
            return await _repository.DeleteAsync(id, userId);
        }

        public async Task<bool> AssignTagToTaskAsync(Guid tagId, Guid taskId, Guid userId)
        {
            var tag = await _repository.GetByIdAsync(tagId);
            if (tag == null || tag.UserId != userId)
            {
                _logger.LogWarning("Assign failed. Tag not found. TagId: {TagId}", tagId);
                throw new KeyNotFoundException("Tag not found.");
            }

            _logger.LogInformation("Assigning tag {TagId} to task {TaskId}", tagId, taskId);
            return await _repository.AssignTagToTaskAsync(tagId, taskId);
        }

        public async Task<bool> RemoveTagFromTaskAsync(Guid tagId, Guid taskId, Guid userId)
        {
            var tag = await _repository.GetByIdAsync(tagId);
            if (tag == null || tag.UserId != userId)
            {
                _logger.LogWarning("Remove failed. Tag not found. TagId: {TagId}", tagId);
                throw new KeyNotFoundException("Tag not found.");
            }

            _logger.LogInformation("Removing tag {TagId} from task {TaskId}", tagId, taskId);
            return await _repository.RemoveTagFromTaskAsync(tagId, taskId);
        }
    }
}
