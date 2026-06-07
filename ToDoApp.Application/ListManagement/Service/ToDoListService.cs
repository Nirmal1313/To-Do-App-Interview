using AutoMapper;
using Microsoft.Extensions.Logging;
using ToDoApp.Application.DTOs.ListDto;
using ToDoApp.Application.ListManagement.Interface;
using ToDoApp.Domain.Entities.Lists;
using ToDoApp.Domain.Interfaces.Lists;
using ToDoApp.Domain.Interfaces.TaskManagment;

namespace ToDoApp.Application.ListManagement.Service
{
    public class ToDoListService : IToDoListService
    {
        private readonly IToDoListRepository _repository;
        private readonly ITaskManagmentRepository _taskRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<ToDoListService> _logger;

        public ToDoListService(IToDoListRepository repository, ITaskManagmentRepository taskRepository, IMapper mapper, ILogger<ToDoListService> logger)
        {
            _repository = repository;
            _taskRepository = taskRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ToDoListDto> CreateAsync(ToDoListDto dto, Guid userId)
        {
            _logger.LogInformation("Creating list for user {UserId}", userId);
            var entity = _mapper.Map<ToDoList>(dto);
            entity.UserId = userId;
            var result = await _repository.CreateAsync(entity);
            _logger.LogInformation("List created. ListId: {ListId}", result.Id);
            return _mapper.Map<ToDoListDto>(result);
        }

        public async Task<ToDoListDto?> GetByIdAsync(Guid id, Guid userId)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null || entity.UserId != userId)
            {
                _logger.LogWarning("List not found or access denied. ListId: {ListId}, UserId: {UserId}", id, userId);
                return null;
            }
            return _mapper.Map<ToDoListDto>(entity);
        }

        public async Task<IEnumerable<ToDoListDto>> GetAllAsync(Guid userId)
        {
            var entities = await _repository.GetByUserIdAsync(userId);
            return _mapper.Map<IEnumerable<ToDoListDto>>(entities);
        }

        public async Task<ToDoListDto?> UpdateAsync(Guid id, ToDoListDto dto, Guid userId)
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null || existing.UserId != userId)
            {
                _logger.LogWarning("Update failed. List not found. ListId: {ListId}", id);
                throw new KeyNotFoundException("List not found.");
            }

            _logger.LogInformation("Updating list {ListId}", id);
            var entity = _mapper.Map<ToDoList>(dto);
            entity.Id = id;
            var result = await _repository.UpdateAsync(entity);
            return _mapper.Map<ToDoListDto>(result);
        }

        public async Task<bool> DeleteAsync(Guid id, Guid userId)
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null || existing.UserId != userId)
            {
                _logger.LogWarning("Delete failed. List not found. ListId: {ListId}", id);
                throw new KeyNotFoundException("List not found.");
            }

            _logger.LogInformation("Deleting list {ListId} and its tasks for user {UserId}", id, userId);

            var deletedTaskCount = await _taskRepository.DeleteByListIdAsync(id, userId);
            _logger.LogInformation("Deleted {Count} tasks for list {ListId}", deletedTaskCount, id);

            return await _repository.DeleteAsync(id, userId);
        }
    }
}
