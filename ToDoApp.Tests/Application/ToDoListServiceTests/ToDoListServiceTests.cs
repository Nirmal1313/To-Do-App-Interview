using AutoMapper;
using Microsoft.Extensions.Logging;
using Moq;
using ToDoApp.Application.DTOs.ListDto;
using ToDoApp.Application.ListManagement.Service;
using ToDoApp.Domain.Entities.Lists;
using ToDoApp.Domain.Interfaces.Lists;
using ToDoApp.Domain.Interfaces.TaskManagment;
using Xunit;

namespace ToDoApp.Tests.Application.ToDoListServiceTests
{
    public class ToDoListServiceTests
    {
        private readonly Mock<IToDoListRepository> _repoMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ILogger<ToDoListService>> _loggerMock;
        private readonly Mock<ITaskManagmentRepository> _taskRepoMock;
        private readonly ToDoListService _service;

        private readonly Guid _userId = Guid.NewGuid();

        public ToDoListServiceTests()
        {
            _repoMock = new Mock<IToDoListRepository>();
            _taskRepoMock = new Mock<ITaskManagmentRepository>();
            _mapperMock = new Mock<IMapper>();
            _loggerMock = new Mock<ILogger<ToDoListService>>();

            _service = new ToDoListService(
                _repoMock.Object,
                _taskRepoMock.Object,
                _mapperMock.Object,
                _loggerMock.Object
            );
        }

        [Fact]
        public async Task CreateAsync_ShouldReturn_ListDto()
        {
            // Arrange
            var dto = new ToDoListDto { Name = "Work" };
            var entity = new ToDoList { Id = Guid.NewGuid(), Name = "Work" };

            _mapperMock.Setup(m => m.Map<ToDoList>(dto)).Returns(entity);
            _repoMock.Setup(r => r.CreateAsync(entity)).ReturnsAsync(entity);
            _mapperMock.Setup(m => m.Map<ToDoListDto>(entity)).Returns(dto);

            // Act
            var result = await _service.CreateAsync(dto, _userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Work", result.Name);
        }

        [Fact]
        public async Task CreateAsync_SetsUserId()
        {
            // Arrange
            var dto = new ToDoListDto { Name = "List" };
            ToDoList? captured = null;

            _mapperMock.Setup(m => m.Map<ToDoList>(dto)).Returns(new ToDoList { Name = "List" });
            _repoMock.Setup(r => r.CreateAsync(It.IsAny<ToDoList>())).Callback<ToDoList>(l => captured = l).ReturnsAsync((ToDoList l) => l);
            _mapperMock.Setup(m => m.Map<ToDoListDto>(It.IsAny<ToDoList>())).Returns(dto);

            // Act
            await _service.CreateAsync(dto, _userId);

            // Assert
            Assert.NotNull(captured);
            Assert.Equal(_userId, captured!.UserId);
        }

        [Fact]
        public async Task GetByIdAsync_WhenOwner_ReturnsDto()
        {
            // Arrange
            var id = Guid.NewGuid();
            var entity = new ToDoList { Id = id, UserId = _userId, Name = "Mine" };
            var dto = new ToDoListDto { Id = id, Name = "Mine" };

            _repoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(entity);
            _mapperMock.Setup(m => m.Map<ToDoListDto>(entity)).Returns(dto);

            // Act
            var result = await _service.GetByIdAsync(id, _userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(id, result!.Id);
        }

        [Fact]
        public async Task GetByIdAsync_WhenNotFound_ReturnsNull()
        {
            // Arrange
            var id = Guid.NewGuid();
            _repoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((ToDoList?)null);

            // Act
            var result = await _service.GetByIdAsync(id, _userId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByIdAsync_WhenDifferentUser_ReturnsNull()
        {
            // Arrange
            var id = Guid.NewGuid();
            var entity = new ToDoList { Id = id, UserId = Guid.NewGuid() };
            _repoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(entity);

            // Act
            var result = await _service.GetByIdAsync(id, _userId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsUserLists()
        {
            // Arrange
            var entities = new List<ToDoList> { new() { UserId = _userId, Name = "A" } };
            var dtos = new List<ToDoListDto> { new() { Name = "A" } };

            _repoMock.Setup(r => r.GetByUserIdAsync(_userId)).ReturnsAsync(entities);
            _mapperMock.Setup(m => m.Map<IEnumerable<ToDoListDto>>(It.IsAny<object>())).Returns(dtos);

            // Act
            var result = await _service.GetAllAsync(_userId);

            // Assert
            Assert.Single(result);
        }

        [Fact]
        public async Task UpdateAsync_WhenOwner_ReturnsDto()
        {
            // Arrange
            var id = Guid.NewGuid();
            var existing = new ToDoList { Id = id, UserId = _userId, Name = "Old" };
            var dto = new ToDoListDto { Name = "New" };
            var entity = new ToDoList { Id = id, Name = "New" };
            var returnedDto = new ToDoListDto { Name = "New" };

            _repoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(existing);
            _mapperMock.Setup(m => m.Map<ToDoList>(dto)).Returns(entity);
            _repoMock.Setup(r => r.UpdateAsync(entity)).ReturnsAsync(entity);
            _mapperMock.Setup(m => m.Map<ToDoListDto>(entity)).Returns(returnedDto);

            // Act
            var result = await _service.UpdateAsync(id, dto, _userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("New", result!.Name);
        }

        [Fact]
        public async Task UpdateAsync_WhenNotFound_ThrowsKeyNotFoundException()
        {
            // Arrange
            var id = Guid.NewGuid();
            _repoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((ToDoList?)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _service.UpdateAsync(id, new ToDoListDto(), _userId));
        }

        [Fact]
        public async Task UpdateAsync_WhenDifferentUser_ThrowsKeyNotFoundException()
        {
            // Arrange
            var id = Guid.NewGuid();
            var entity = new ToDoList { Id = id, UserId = Guid.NewGuid() };
            _repoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(entity);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _service.UpdateAsync(id, new ToDoListDto(), _userId));
        }

        [Fact]
        public async Task DeleteAsync_WhenOwner_ReturnsTrue()
        {
            // Arrange
            var id = Guid.NewGuid();
            var entity = new ToDoList { Id = id, UserId = _userId };

            _repoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(entity);
            _repoMock.Setup(r => r.DeleteAsync(id, It.IsAny<Guid>())).ReturnsAsync(true);

            // Act
            var result = await _service.DeleteAsync(id, _userId);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task DeleteAsync_WhenNotFound_ThrowsKeyNotFoundException()
        {
            // Arrange
            var id = Guid.NewGuid();
            _repoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((ToDoList?)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _service.DeleteAsync(id, _userId));
        }

        [Fact]
        public async Task DeleteAsync_WhenDifferentUser_ThrowsKeyNotFoundException()
        {
            // Arrange
            var id = Guid.NewGuid();
            var entity = new ToDoList { Id = id, UserId = Guid.NewGuid() };
            _repoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(entity);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _service.DeleteAsync(id, _userId));
        }
    }
}
