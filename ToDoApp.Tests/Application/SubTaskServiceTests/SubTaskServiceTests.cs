using AutoMapper;
using Microsoft.Extensions.Logging;
using Moq;
using ToDoApp.Application.SubTaskManagement.Service;
using ToDoApp.Domain.Entities.SubTasks;
using ToDoApp.Domain.Entities.ToDoTask.TaskDetails;
using ToDoApp.Domain.Interfaces.SubTasks;
using ToDoApp.Domain.Interfaces.TaskManagment;
using Xunit;
using SubTaskDtoClass = ToDoApp.Application.DTOs.SubTaskDto.SubTaskDto;

namespace ToDoApp.Tests.Application.SubTaskServiceTests
{
    public class SubTaskServiceTests
    {
        private readonly Mock<ISubTaskRepository> _repoMock;
        private readonly Mock<ITaskManagmentRepository> _taskRepoMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ILogger<SubTaskService>> _loggerMock;
        private readonly SubTaskService _service;
        private readonly Guid _userId = Guid.NewGuid();

        public SubTaskServiceTests()
        {
            _repoMock = new Mock<ISubTaskRepository>();
            _taskRepoMock = new Mock<ITaskManagmentRepository>();
            _mapperMock = new Mock<IMapper>();
            _loggerMock = new Mock<ILogger<SubTaskService>>();

            _service = new SubTaskService(
                _repoMock.Object,
                _taskRepoMock.Object,
                _mapperMock.Object,
                _loggerMock.Object
            );
        }

        [Fact]
        public async Task CreateAsync_ShouldReturn_SubTaskDto()
        {
            // Arrange
            var parentId = Guid.NewGuid();
            var dto = new SubTaskDtoClass { Title = "Step 1", ParentTaskId = parentId };
            var entity = new SubTask { Id = Guid.NewGuid(), Title = "Step 1", ParentTaskId = parentId };

            _mapperMock.Setup(m => m.Map<SubTask>(dto)).Returns(entity);
            _repoMock.Setup(r => r.CreateAsync(entity)).ReturnsAsync(entity);
            _mapperMock.Setup(m => m.Map<SubTaskDtoClass>(entity)).Returns(dto);

            // Act
            var result = await _service.CreateAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Step 1", result.Title);
        }

        [Fact]
        public async Task GetByTaskIdAsync_ReturnsMappedDtos()
        {
            // Arrange
            var parentId = Guid.NewGuid();
            var entities = new List<SubTask> { new() { ParentTaskId = parentId, Title = "Sub" } };
            var dtos = new List<SubTaskDtoClass> { new() { Title = "Sub" } };

            _repoMock.Setup(r => r.GetByParentTaskIdAsync(parentId)).ReturnsAsync(entities);
            _mapperMock.Setup(m => m.Map<IEnumerable<SubTaskDtoClass>>(It.IsAny<object>())).Returns(dtos);

            // Act
            var result = await _service.GetByTaskIdAsync(parentId);

            // Assert
            Assert.Single(result);
        }

        [Fact]
        public async Task UpdateAsync_WhenFound_ReturnsDto()
        {
            // Arrange
            var id = Guid.NewGuid();
            var dto = new SubTaskDtoClass { Title = "Updated", IsCompleted = true };
            var entity = new SubTask { Id = id, Title = "Updated" };
            var returned = new SubTask { Id = id, Title = "Updated", IsCompleted = true };
            var returnedDto = new SubTaskDtoClass { Title = "Updated", IsCompleted = true };

            _mapperMock.Setup(m => m.Map<SubTask>(dto)).Returns(entity);
            _repoMock.Setup(r => r.UpdateAsync(entity)).ReturnsAsync(returned);
            _mapperMock.Setup(m => m.Map<SubTaskDtoClass>(returned)).Returns(returnedDto);

            // Act
            var result = await _service.UpdateAsync(id, dto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result!.IsCompleted);
        }

        [Fact]
        public async Task UpdateAsync_WhenNotFound_ThrowsKeyNotFoundException()
        {
            // Arrange
            var id = Guid.NewGuid();
            var dto = new SubTaskDtoClass { Title = "X" };

            _mapperMock.Setup(m => m.Map<SubTask>(dto)).Returns(new SubTask());
            _repoMock.Setup(r => r.UpdateAsync(It.IsAny<SubTask>())).ReturnsAsync((SubTask?)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _service.UpdateAsync(id, dto));
        }

        [Fact]
        public async Task DeleteAsync_WhenOwner_ReturnsTrue()
        {
            // Arrange
            var id = Guid.NewGuid();
            var parentTaskId = Guid.NewGuid();
            var subTask = new SubTask { Id = id, Title = "Sub", ParentTaskId = parentTaskId };
            var parentTask = new ToDoTasks { Id = parentTaskId, UserId = _userId };

            _repoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(subTask);
            _taskRepoMock.Setup(r => r.GetTaskByIdAsync(parentTaskId)).ReturnsAsync(parentTask);
            _repoMock.Setup(r => r.DeleteAsync(id, _userId)).ReturnsAsync(true);

            // Act
            var result = await _service.DeleteAsync(id, _userId);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task DeleteAsync_WhenSubTaskNotFound_ThrowsKeyNotFoundException()
        {
            // Arrange
            var id = Guid.NewGuid();
            _repoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((SubTask?)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _service.DeleteAsync(id, _userId));
        }

        [Fact]
        public async Task DeleteAsync_WhenUnauthorized_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            var id = Guid.NewGuid();
            var parentTaskId = Guid.NewGuid();
            var subTask = new SubTask { Id = id, Title = "Sub", ParentTaskId = parentTaskId };
            var parentTask = new ToDoTasks { Id = parentTaskId, UserId = Guid.NewGuid() };

            _repoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(subTask);
            _taskRepoMock.Setup(r => r.GetTaskByIdAsync(parentTaskId)).ReturnsAsync(parentTask);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                _service.DeleteAsync(id, _userId));
        }
    }
}
