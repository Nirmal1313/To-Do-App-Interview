using AutoMapper;
using Microsoft.Extensions.Logging;
using ToDoApp.Application.DTOs.SubTaskDto;
using ToDoApp.Application.SubTaskManagement.Interface;
using ToDoApp.Domain.Entities.SubTasks;
using ToDoApp.Domain.Interfaces.SubTasks;
using ToDoApp.Domain.Interfaces.TaskManagment;

namespace ToDoApp.Application.SubTaskManagement.Service
{
    public class SubTaskService : ISubTaskService
    {
        private readonly ISubTaskRepository _repository;
        private readonly ITaskManagmentRepository _taskRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<SubTaskService> _logger;

        public SubTaskService(ISubTaskRepository repository, ITaskManagmentRepository taskRepository, IMapper mapper, ILogger<SubTaskService> logger)
        {
            _repository = repository;
            _taskRepository = taskRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<SubTaskDto> CreateAsync(SubTaskDto dto)
        {
            _logger.LogInformation("Creating subtask for parent task {ParentTaskId}", dto.ParentTaskId);
            var entity = _mapper.Map<SubTask>(dto);
            var result = await _repository.CreateAsync(entity);
            _logger.LogInformation("SubTask created. SubTaskId: {SubTaskId}", result.Id);
            return _mapper.Map<SubTaskDto>(result);
        }

        public async Task<IEnumerable<SubTaskDto>> GetByTaskIdAsync(Guid parentTaskId)
        {
            var entities = await _repository.GetByParentTaskIdAsync(parentTaskId);
            return _mapper.Map<IEnumerable<SubTaskDto>>(entities);
        }

        public async Task<SubTaskDto?> UpdateAsync(Guid id, SubTaskDto dto)
        {
            _logger.LogInformation("Updating subtask {SubTaskId}", id);
            var entity = _mapper.Map<SubTask>(dto);
            entity.Id = id;
            var result = await _repository.UpdateAsync(entity);
            if (result == null)
            {
                _logger.LogWarning("Update failed. SubTask not found. SubTaskId: {SubTaskId}", id);
                throw new KeyNotFoundException("SubTask not found.");
            }
            return _mapper.Map<SubTaskDto>(result);
        }

        public async Task<bool> DeleteAsync(Guid id, Guid userId)
        {
            _logger.LogInformation("Deleting subtask {SubTaskId}", id);

            var subTask = await _repository.GetByIdAsync(id);
            if (subTask == null)
            {
                _logger.LogWarning("Delete failed. SubTask not found. SubTaskId: {SubTaskId}", id);
                throw new KeyNotFoundException("SubTask not found.");
            }

            var parentTask = await _taskRepository.GetTaskByIdAsync(subTask.ParentTaskId);
            if (parentTask == null || parentTask.UserId != userId)
            {
                _logger.LogWarning("Unauthorized subtask delete attempt. SubTaskId: {SubTaskId}, UserId: {UserId}", id, userId);
                throw new UnauthorizedAccessException("Not allowed to delete this subtask.");
            }

            var deleted = await _repository.DeleteAsync(id, userId);
            if (!deleted)
                throw new KeyNotFoundException("SubTask not found.");

            return true;
        }
    }
}
