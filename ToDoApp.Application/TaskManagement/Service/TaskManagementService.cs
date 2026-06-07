using AutoMapper;
using Microsoft.Extensions.Logging;
using ToDoApp.Application.DTOs.TaskDto;
using ToDoApp.Application.TaskManagement.Interface;
using ToDoApp.Domain.Common.Enums;
using ToDoApp.Domain.Entities.ToDoTask.TaskDetails;
using ToDoApp.Domain.Interfaces.Tags;
using ToDoApp.Domain.Interfaces.TaskManagment;
using TaskStatus = ToDoApp.Domain.Common.Enums.TaskStatus;

namespace ToDoApp.Application.TaskManagement.Service
{
    public class TaskManagementService : ITaskManagementService
    {
        private readonly ITaskManagmentRepository _taskRepository;
        private readonly ITagRepository _tagRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<TaskManagementService> _logger;

        public TaskManagementService(ITaskManagmentRepository taskRepository, ITagRepository tagRepository, IMapper mapper, ILogger<TaskManagementService> logger)
        {
            _taskRepository = taskRepository;
            _tagRepository = tagRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ToDoTaskDto> AddTaskAsync(ToDoTaskDto dto, Guid userId)
        {
            try
            {
                _logger.LogInformation("Creating task for user {UserId}", userId);

                var entity = _mapper.Map<ToDoTasks>(dto);

                entity.UserId = userId;
                entity.Status = TaskStatus.Created;

                var result = await _taskRepository.CreateTaskAsync(entity);

                _logger.LogInformation("Task created successfully. TaskId: {TaskId}", result.Id);

                return _mapper.Map<ToDoTaskDto>(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while creating task for user {UserId}", userId);
                throw;
            }
        }

        public async Task<List<ToDoTaskDto>> GetAllAsync(Guid userId)
        {
            var tasks = await _taskRepository.GetTasksByUserIdAsync(userId);
            return _mapper.Map<List<ToDoTaskDto>>(tasks.ToList());
        }

        public async Task<ToDoTaskDto?> GetTaskByIdAsync(Guid id, Guid userId)
        {
            var task = await _taskRepository.GetTaskByIdAsync(id);

            if (task == null)
            {
                _logger.LogWarning("Task not found. TaskId: {TaskId}", id);
                return null;
            }

            if (task.UserId != userId)
            {
                _logger.LogWarning("Unauthorized access. TaskId: {TaskId}, UserId: {UserId}", id, userId);
                throw new Exception("You are not allowed to access this task.");
            }

            return _mapper.Map<ToDoTaskDto>(task);
        }

        public async Task<List<ToDoTaskDto?>> GetTaskByTitleAsync(string title, Guid userId)
        {
            var taskDetails = await _taskRepository.GetTaskByNameAsync(title);

            if (taskDetails == null || !taskDetails.Any())
            {
                _logger.LogWarning("No tasks found with title '{Title}'", title);
                return new List<ToDoTaskDto?>();
            }

            var userTasks = taskDetails.Where(x => x != null && x.UserId == userId && !x.IsDeleted).ToList();

            return _mapper.Map<List<ToDoTaskDto?>>(userTasks);
        }

        public async Task<List<ToDoTaskDto>> GetTasksByStatusAsync(Guid userId, TaskStatus taskStatus)
        {
            var tasks = await _taskRepository.GetTasksByStatusAsync(taskStatus, userId);
            return _mapper.Map<List<ToDoTaskDto>>(tasks.ToList());
        }

        public async Task<List<ToDoTaskDto>> GetTasksByPriorityAsync(Guid userId, TaskPriority taskPriority)
        {
            var tasks = await _taskRepository.GetTasksByPriorityAsync(taskPriority, userId);
            return _mapper.Map<List<ToDoTaskDto>>(tasks.ToList());
        }

        public async Task<List<ToDoTaskDto>> GetTasksByListIdAsync(Guid listId, Guid userId)
        {
            var taskDetails = await _taskRepository.GetTasksByListIdAsync(listId);
            var userTasks = taskDetails.Where(x => x.UserId == userId).ToList();
            return _mapper.Map<List<ToDoTaskDto>>(userTasks);
        }

        public async Task ReplaceTaskTagsAsync(Guid taskId, List<Guid> tagIds, Guid userId)
        {
            var task = await _taskRepository.GetTaskByIdAsync(taskId);

            if (task == null || task.UserId != userId)
                throw new KeyNotFoundException("Task not found.");

            await _tagRepository.ReplaceTaskTagsAsync(taskId, tagIds);
        }

        public async Task<ToDoTaskDto?> UpdateTaskAsync(Guid id, ToDoTaskDto dto, Guid userId)
        {
            try
            {
                var task = await _taskRepository.GetTaskByIdAsync(id);

                if (task == null)
                {
                    _logger.LogWarning("Update failed. Task not found {TaskId}", id);
                    throw new KeyNotFoundException("Task not found.");
                }

                if (task.UserId != userId)
                {
                    _logger.LogWarning("Unauthorized update attempt. TaskId: {TaskId}, UserId: {UserId}", id, userId);
                    throw new Exception("Not allowed to update this task.");
                }

                _logger.LogInformation("Updating task {TaskId}", id);

                task.Title = dto.Title;
                task.Description = dto.Description;
                task.Priority = dto.Priority;
                task.Status = dto.Status;
                task.ListId = dto.ListId;
                task.UpdatedBy = userId;
                task.LastUpdatedDate = DateTime.UtcNow;

                await _taskRepository.UpdateTaskAsync(task);

                return _mapper.Map<ToDoTaskDto>(task);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating task {TaskId}", id);
                throw;
            }
        }

        public async Task<bool> DeleteTaskAsync(Guid id, Guid userId)
        {
            try
            {
                var task = await _taskRepository.GetTaskByIdAsync(id);

                if (task == null)
                {
                    _logger.LogWarning("Delete failed. Task not found {TaskId}", id);
                    throw new KeyNotFoundException("Task not found.");
                }

                if (task.UserId != userId)
                {
                    _logger.LogWarning("Unauthorized delete attempt. TaskId: {TaskId}, UserId: {UserId}", id, userId);
                    throw new Exception("Not allowed to delete this task.");
                }

                await _taskRepository.DeleteTaskAsync(task.Id, userId);

                _logger.LogInformation("Task deleted successfully {TaskId}", id);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting task {TaskId}", id);
                throw;
            }
        }

    }
}
