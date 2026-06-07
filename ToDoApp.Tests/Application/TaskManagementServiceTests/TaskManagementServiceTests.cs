using AutoMapper;
using Microsoft.Extensions.Logging;
using Moq;
using ToDoApp.Application.DTOs.TaskDto;
using ToDoApp.Domain.Common.Enums;
using ToDoApp.Domain.Entities.ToDoTask.TaskDetails;
using ToDoApp.Domain.Interfaces.Tags;
using ToDoApp.Application.TaskManagement.Service;
using ToDoApp.Domain.Interfaces.TaskManagment;
using Xunit;
using DomainTaskStatus = ToDoApp.Domain.Common.Enums.TaskStatus;

namespace ToDoApp.Tests.Application.TaskManagement
{

    public class TaskManagmentServiceTests
    {
        private readonly Mock<ITaskManagmentRepository> _repoMock;
        private readonly Mock<ITagRepository> _tagRepoMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ILogger<TaskManagementService>> _loggerMock;
        private readonly TaskManagementService _service;

        private readonly Guid _userId = Guid.NewGuid();

        public TaskManagmentServiceTests()
        {
            _repoMock = new Mock<ITaskManagmentRepository>();
            _tagRepoMock = new Mock<ITagRepository>();
            _mapperMock = new Mock<IMapper>();
            _loggerMock = new Mock<ILogger<TaskManagementService>>();

            _service = new TaskManagementService(
                _repoMock.Object,
                _tagRepoMock.Object,
                _mapperMock.Object,
                _loggerMock.Object
            );
        }

        [Fact]
        public async Task AddTaskAsync_ShouldReturn_TaskDto()
        {
            // Arrange
            var dto = new ToDoTaskDto { Title = "Test Task", Description = "desc" };
            var entity = new ToDoTasks { Id = Guid.NewGuid(), Title = "Test Task" };

            _mapperMock.Setup(m => m.Map<ToDoTasks>(dto)).Returns(entity);
            _repoMock.Setup(r => r.CreateTaskAsync(entity)).ReturnsAsync(entity);
            _mapperMock.Setup(m => m.Map<ToDoTaskDto>(entity)).Returns(dto);

            // Act
            var result = await _service.AddTaskAsync(dto, _userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Test Task", result.Title);
        }

        [Fact]
        public async Task AddTaskAsync_SetsCreatedByAndStatus()
        {
            // Arrange
            var dto = new ToDoTaskDto { Title = "New Task", Description = "desc" };
            ToDoTasks? captured = null;

            _mapperMock.Setup(m => m.Map<ToDoTasks>(dto)).Returns(new ToDoTasks { Title = "New Task" });
            _repoMock.Setup(r => r.CreateTaskAsync(It.IsAny<ToDoTasks>())).Callback<ToDoTasks>(t => captured = t).ReturnsAsync((ToDoTasks t) => t);

            _mapperMock.Setup(m => m.Map<ToDoTaskDto>(It.IsAny<ToDoTasks>())).Returns(dto);

            // Act
            await _service.AddTaskAsync(dto, _userId);

            // Assert
            Assert.NotNull(captured);
            Assert.Equal(_userId, captured!.UserId);
            Assert.Equal(DomainTaskStatus.Created, captured.Status);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsCurrentUserTasks()
        {
            // Arrange
            var tasks = new List<ToDoTasks> { new() { Id = Guid.NewGuid(), UserId = _userId } };
            var expectedDtos = new List<ToDoTaskDto> { new() { Title = "Mine" } };

            _repoMock.Setup(r => r.GetTasksByUserIdAsync(_userId)).ReturnsAsync(tasks);
            _mapperMock.Setup(m => m.Map<List<ToDoTaskDto>>(It.IsAny<object>())).Returns(expectedDtos);

            // Act
            var result = await _service.GetAllAsync(_userId);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
        }

        [Fact]
        public async Task GetAllAsync_WhenNoTasksForUser_ReturnsEmptyList()
        {
            // Arrange
            _repoMock.Setup(r => r.GetTasksByUserIdAsync(_userId)).ReturnsAsync(new List<ToDoTasks>());
            _mapperMock.Setup(m => m.Map<List<ToDoTaskDto>>(It.IsAny<object>())).Returns(new List<ToDoTaskDto>());

            // Act
            var result = await _service.GetAllAsync(_userId);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetTaskByIdAsync_WhenOwner_ReturnsDto()
        {
            // Arrange
            var taskId = Guid.NewGuid();
            var entity = new ToDoTasks { Id = taskId, UserId = _userId };
            var dto = new ToDoTaskDto { Id = taskId };

            _repoMock.Setup(r => r.GetTaskByIdAsync(taskId)).ReturnsAsync(entity);
            _mapperMock.Setup(m => m.Map<ToDoTaskDto>(entity)).Returns(dto);

            // Act
            var result = await _service.GetTaskByIdAsync(taskId, _userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(taskId, result!.Id);
        }

        [Fact]
        public async Task GetTaskByIdAsync_WhenNotFound_ReturnsNull()
        {
            // Arrange
            var taskId = Guid.NewGuid();
            _repoMock.Setup(r => r.GetTaskByIdAsync(taskId)).ReturnsAsync((ToDoTasks?)null);

            // Act
            var result = await _service.GetTaskByIdAsync(taskId, _userId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetTaskByIdAsync_WhenOwnedByOtherUser_ThrowsException()
        {
            // Arrange
            var taskId = Guid.NewGuid();
            var entity = new ToDoTasks { Id = taskId, UserId = Guid.NewGuid() };
            _repoMock.Setup(r => r.GetTaskByIdAsync(taskId)).ReturnsAsync(entity);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _service.GetTaskByIdAsync(taskId, _userId));
        }

        [Fact]
        public async Task GetTasksByStatusAsync_ReturnsMatchingTasks()
        {
            // Arrange
            var tasks = new List<ToDoTasks> { new() { Status = DomainTaskStatus.Created, UserId = _userId } };
            var dtos = new List<ToDoTaskDto> { new() };

            _repoMock.Setup(r => r.GetTasksByStatusAsync(DomainTaskStatus.Created, _userId)).ReturnsAsync(tasks);
            _mapperMock.Setup(m => m.Map<List<ToDoTaskDto>>(It.IsAny<object>())).Returns(dtos);

            // Act
            var result = await _service.GetTasksByStatusAsync(_userId, DomainTaskStatus.Created);

            // Assert
            Assert.Single(result);
        }

        [Fact]
        public async Task GetTasksByStatusAsync_WhenNoTasks_ReturnsEmpty()
        {
            // Arrange
            _repoMock.Setup(r => r.GetTasksByStatusAsync(DomainTaskStatus.Created, _userId))
                .ReturnsAsync(new List<ToDoTasks>());
            _mapperMock.Setup(m => m.Map<List<ToDoTaskDto>>(It.IsAny<object>())).Returns(new List<ToDoTaskDto>());

            // Act
            var result = await _service.GetTasksByStatusAsync(_userId, DomainTaskStatus.Created);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetTasksByPriorityAsync_ReturnsMatchingTasks()
        {
            // Arrange
            var tasks = new List<ToDoTasks> { new() { Priority = TaskPriority.High, UserId = _userId } };
            var dtos = new List<ToDoTaskDto> { new() };

            _repoMock.Setup(r => r.GetTasksByPriorityAsync(TaskPriority.High, _userId)).ReturnsAsync(tasks);
            _mapperMock.Setup(m => m.Map<List<ToDoTaskDto>>(It.IsAny<object>())).Returns(dtos);

            // Act
            var result = await _service.GetTasksByPriorityAsync(_userId, TaskPriority.High);

            // Assert
            Assert.Single(result);
        }


        [Fact]
        public async Task UpdateTaskAsync_WhenOwner_ReturnsUpdatedDto()
        {
            // Arrange
            var taskId = Guid.NewGuid();
            var existing = new ToDoTasks { Id = taskId, UserId = _userId, Title = "Old" };
            var dto = new ToDoTaskDto { Title = "New", Description = "desc" };
            var returnedDto = new ToDoTaskDto { Title = "New" };

            _repoMock.Setup(r => r.GetTaskByIdAsync(taskId)).ReturnsAsync(existing);
            _repoMock.Setup(r => r.UpdateTaskAsync(existing)).ReturnsAsync(existing);
            _mapperMock.Setup(m => m.Map<ToDoTaskDto>(existing)).Returns(returnedDto);

            // Act
            var result = await _service.UpdateTaskAsync(taskId, dto, _userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("New", result!.Title);
        }

        [Fact]
        public async Task UpdateTaskAsync_WhenTaskNotFound_ThrowsKeyNotFoundException()
        {
            // Arrange
            var taskId = Guid.NewGuid();
            _repoMock.Setup(r => r.GetTaskByIdAsync(taskId)).ReturnsAsync((ToDoTasks?)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _service.UpdateTaskAsync(taskId, new ToDoTaskDto(), _userId));
        }

        [Fact]
        public async Task UpdateTaskAsync_WhenOwnedByOtherUser_ThrowsException()
        {
            // Arrange
            var taskId = Guid.NewGuid();
            var existing = new ToDoTasks { Id = taskId, UserId = Guid.NewGuid() };
            _repoMock.Setup(r => r.GetTaskByIdAsync(taskId)).ReturnsAsync(existing);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() =>
                _service.UpdateTaskAsync(taskId, new ToDoTaskDto(), _userId));
        }

        [Fact]
        public async Task DeleteTaskAsync_WhenOwner_ReturnsTrue()
        {
            // Arrange
            var taskId = Guid.NewGuid();
            var entity = new ToDoTasks { Id = taskId, UserId = _userId };

            _repoMock.Setup(r => r.GetTaskByIdAsync(taskId)).ReturnsAsync(entity);
            _repoMock.Setup(r => r.DeleteTaskAsync(taskId, It.IsAny<Guid>())).ReturnsAsync(entity);

            // Act
            var result = await _service.DeleteTaskAsync(taskId, _userId);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task DeleteTaskAsync_WhenTaskNotFound_ThrowsKeyNotFoundException()
        {
            // Arrange
            var taskId = Guid.NewGuid();
            _repoMock.Setup(r => r.GetTaskByIdAsync(taskId)).ReturnsAsync((ToDoTasks?)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _service.DeleteTaskAsync(taskId, _userId));
        }

        [Fact]
        public async Task DeleteTaskAsync_WhenOwnedByOtherUser_ThrowsException()
        {
            // Arrange
            var taskId = Guid.NewGuid();
            var entity = new ToDoTasks { Id = taskId, UserId = Guid.NewGuid() };
            _repoMock.Setup(r => r.GetTaskByIdAsync(taskId)).ReturnsAsync(entity);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() =>
                _service.DeleteTaskAsync(taskId, _userId));
        }

        [Fact]
        public async Task DeleteTaskAsync_CallsRepositoryDelete()
        {
            // Arrange
            var taskId = Guid.NewGuid();
            var entity = new ToDoTasks { Id = taskId, UserId = _userId };

            _repoMock.Setup(r => r.GetTaskByIdAsync(taskId)).ReturnsAsync(entity);
            _repoMock.Setup(r => r.DeleteTaskAsync(taskId, It.IsAny<Guid>())).ReturnsAsync(entity);

            // Act
            await _service.DeleteTaskAsync(taskId, _userId);

            // Assert
            _repoMock.Verify(r => r.DeleteTaskAsync(taskId, It.IsAny<Guid>()), Times.Once);
        }

        [Fact]
        public async Task ReplaceTaskTagsAsync_DelegatesToRepository()
        {
            // Arrange
            var taskId = Guid.NewGuid();
            var entity = new ToDoTasks { Id = taskId, UserId = _userId };
            var tagIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };

            _repoMock.Setup(r => r.GetTaskByIdAsync(taskId)).ReturnsAsync(entity);
            _tagRepoMock.Setup(r => r.ReplaceTaskTagsAsync(taskId, tagIds)).Returns(Task.CompletedTask);

            // Act
            await _service.ReplaceTaskTagsAsync(taskId, tagIds, _userId);

            // Assert
            _tagRepoMock.Verify(r => r.ReplaceTaskTagsAsync(taskId, tagIds), Times.Once);
        }

        [Fact]
        public async Task ReplaceTaskTagsAsync_WhenTaskNotFound_ThrowsKeyNotFoundException()
        {
            // Arrange
            var taskId = Guid.NewGuid();
            _repoMock.Setup(r => r.GetTaskByIdAsync(taskId)).ReturnsAsync((ToDoTasks?)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _service.ReplaceTaskTagsAsync(taskId, new List<Guid>(), _userId));
        }

        [Fact]
        public async Task ReplaceTaskTagsAsync_WhenOwnedByOtherUser_ThrowsKeyNotFoundException()
        {
            // Arrange
            var taskId = Guid.NewGuid();
            var entity = new ToDoTasks { Id = taskId, UserId = Guid.NewGuid() };
            _repoMock.Setup(r => r.GetTaskByIdAsync(taskId)).ReturnsAsync(entity);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _service.ReplaceTaskTagsAsync(taskId, new List<Guid>(), _userId));
        }

        [Fact]
        public async Task ReplaceTaskTagsAsync_WithEmptyList_CallsRepositoryWithEmptySet()
        {
            // Arrange
            var taskId = Guid.NewGuid();
            var entity = new ToDoTasks { Id = taskId, UserId = _userId };
            var emptyList = new List<Guid>();

            _repoMock.Setup(r => r.GetTaskByIdAsync(taskId)).ReturnsAsync(entity);
            _tagRepoMock.Setup(r => r.ReplaceTaskTagsAsync(taskId, emptyList)).Returns(Task.CompletedTask);

            // Act
            await _service.ReplaceTaskTagsAsync(taskId, emptyList, _userId);

            // Assert
            _tagRepoMock.Verify(r => r.ReplaceTaskTagsAsync(taskId, emptyList), Times.Once);
        }
    }
}
