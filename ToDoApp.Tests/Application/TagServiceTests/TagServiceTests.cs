using AutoMapper;
using Microsoft.Extensions.Logging;
using Moq;
using ToDoApp.Application.TagManagement.Service;
using ToDoApp.Domain.Entities.Tags;
using ToDoApp.Domain.Interfaces.Tags;
using Xunit;
using TagDtoClass = ToDoApp.Application.DTOs.TagDto.TagDto;

namespace ToDoApp.Tests.Application.TagServiceTests
{
    public class TagServiceTests
    {
        private readonly Mock<ITagRepository> _repoMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ILogger<TagService>> _loggerMock;
        private readonly TagService _service;

        private readonly Guid _userId = Guid.NewGuid();

        public TagServiceTests()
        {
            _repoMock = new Mock<ITagRepository>();
            _mapperMock = new Mock<IMapper>();
            _loggerMock = new Mock<ILogger<TagService>>();

            _service = new TagService(
                _repoMock.Object,
                _mapperMock.Object,
                _loggerMock.Object
            );
        }

        [Fact]
        public async Task CreateAsync_ShouldReturn_TagDto()
        {
            // Arrange
            var dto = new TagDtoClass { Name = "Bug" };
            var entity = new Tag { Id = Guid.NewGuid(), Name = "Bug" };

            _mapperMock.Setup(m => m.Map<Tag>(dto)).Returns(entity);
            _repoMock.Setup(r => r.CreateAsync(entity)).ReturnsAsync(entity);
            _mapperMock.Setup(m => m.Map<TagDtoClass>(entity)).Returns(dto);

            // Act
            var result = await _service.CreateAsync(dto, _userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Bug", result.Name);
        }

        [Fact]
        public async Task CreateAsync_SetsUserId()
        {
            // Arrange
            var dto = new TagDtoClass { Name = "Tag" };
            Tag? captured = null;

            _mapperMock.Setup(m => m.Map<Tag>(dto)).Returns(new Tag { Name = "Tag" });
            _repoMock.Setup(r => r.CreateAsync(It.IsAny<Tag>())).Callback<Tag>(t => captured = t).ReturnsAsync((Tag t) => t);
            _mapperMock.Setup(m => m.Map<TagDtoClass>(It.IsAny<Tag>())).Returns(dto);

            // Act
            await _service.CreateAsync(dto, _userId);

            // Assert
            Assert.NotNull(captured);
            Assert.Equal(_userId, captured!.UserId);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsUserTags()
        {
            // Arrange
            var entities = new List<Tag> { new() { UserId = _userId, Name = "Feature" } };
            var dtos = new List<TagDtoClass> { new() { Name = "Feature" } };

            _repoMock.Setup(r => r.GetByUserIdAsync(_userId)).ReturnsAsync(entities);
            _mapperMock.Setup(m => m.Map<IEnumerable<TagDtoClass>>(It.IsAny<object>())).Returns(dtos);

            // Act
            var result = await _service.GetAllAsync(_userId);

            // Assert
            Assert.Single(result);
        }

        [Fact]
        public async Task DeleteAsync_WhenOwner_ReturnsTrue()
        {
            // Arrange
            var id = Guid.NewGuid();
            var tag = new Tag { Id = id, UserId = _userId };

            _repoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(tag);
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
            _repoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((Tag?)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _service.DeleteAsync(id, _userId));
        }

        [Fact]
        public async Task DeleteAsync_WhenDifferentUser_ThrowsKeyNotFoundException()
        {
            // Arrange
            var id = Guid.NewGuid();
            var tag = new Tag { Id = id, UserId = Guid.NewGuid() };
            _repoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(tag);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _service.DeleteAsync(id, _userId));
        }

        [Fact]
        public async Task AssignTagToTaskAsync_WhenOwner_ReturnsTrue()
        {
            // Arrange
            var tagId = Guid.NewGuid();
            var taskId = Guid.NewGuid();
            var tag = new Tag { Id = tagId, UserId = _userId };

            _repoMock.Setup(r => r.GetByIdAsync(tagId)).ReturnsAsync(tag);
            _repoMock.Setup(r => r.AssignTagToTaskAsync(tagId, taskId)).ReturnsAsync(true);

            // Act
            var result = await _service.AssignTagToTaskAsync(tagId, taskId, _userId);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task AssignTagToTaskAsync_WhenTagNotFound_ThrowsKeyNotFoundException()
        {
            // Arrange
            var tagId = Guid.NewGuid();
            _repoMock.Setup(r => r.GetByIdAsync(tagId)).ReturnsAsync((Tag?)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _service.AssignTagToTaskAsync(tagId, Guid.NewGuid(), _userId));
        }

        [Fact]
        public async Task AssignTagToTaskAsync_WhenDifferentUser_ThrowsKeyNotFoundException()
        {
            // Arrange
            var tagId = Guid.NewGuid();
            var tag = new Tag { Id = tagId, UserId = Guid.NewGuid() };
            _repoMock.Setup(r => r.GetByIdAsync(tagId)).ReturnsAsync(tag);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _service.AssignTagToTaskAsync(tagId, Guid.NewGuid(), _userId));
        }

        [Fact]
        public async Task RemoveTagFromTaskAsync_WhenOwner_ReturnsTrue()
        {
            // Arrange
            var tagId = Guid.NewGuid();
            var taskId = Guid.NewGuid();
            var tag = new Tag { Id = tagId, UserId = _userId };

            _repoMock.Setup(r => r.GetByIdAsync(tagId)).ReturnsAsync(tag);
            _repoMock.Setup(r => r.RemoveTagFromTaskAsync(tagId, taskId)).ReturnsAsync(true);

            // Act
            var result = await _service.RemoveTagFromTaskAsync(tagId, taskId, _userId);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task RemoveTagFromTaskAsync_WhenTagNotFound_ThrowsKeyNotFoundException()
        {
            // Arrange
            var tagId = Guid.NewGuid();
            _repoMock.Setup(r => r.GetByIdAsync(tagId)).ReturnsAsync((Tag?)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _service.RemoveTagFromTaskAsync(tagId, Guid.NewGuid(), _userId));
        }

        [Fact]
        public async Task RemoveTagFromTaskAsync_WhenDifferentUser_ThrowsKeyNotFoundException()
        {
            // Arrange
            var tagId = Guid.NewGuid();
            var tag = new Tag { Id = tagId, UserId = Guid.NewGuid() };
            _repoMock.Setup(r => r.GetByIdAsync(tagId)).ReturnsAsync(tag);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _service.RemoveTagFromTaskAsync(tagId, Guid.NewGuid(), _userId));
        }
    }
}
