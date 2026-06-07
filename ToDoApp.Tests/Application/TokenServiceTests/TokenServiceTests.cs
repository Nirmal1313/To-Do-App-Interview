using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using ToDoApp.Application.Auth.Interface;
using ToDoApp.Application.Auth.Service;
using ToDoApp.Domain.Entities.User;
using Xunit;

namespace ToDoApp.Tests.Application.TokenServiceTests
{
    public class TokenServiceTests
    {
        private readonly Mock<IConfiguration> _configMock;
        private readonly Mock<ITokenStore> _tokenStoreMock;
        private readonly Mock<ILogger<TokenService>> _loggerMock;
        private readonly TokenService _service;

        private readonly User _testUser = new()
        {
            Id = Guid.NewGuid(),
            Email = "test@test.com",
            FirstName = "Test"
        };

        public TokenServiceTests()
        {
            _configMock = new Mock<IConfiguration>();
            _tokenStoreMock = new Mock<ITokenStore>();
            _loggerMock = new Mock<ILogger<TokenService>>();

            // JWT config values the token service reads at runtime
            _configMock.Setup(c => c["Jwt:Key"]).Returns("TestSecretKeyForJwtThatIsAtLeast32Chars!");
            _configMock.Setup(c => c["Jwt:Issuer"]).Returns("ToDoApp");
            _configMock.Setup(c => c["Jwt:Audience"]).Returns("ToDoAppUsers");

            _tokenStoreMock.Setup(s => s.AddTokenAsync(It.IsAny<string>())).Returns(Task.CompletedTask);
            _tokenStoreMock.Setup(s => s.RevokeTokenAsync(It.IsAny<string>())).Returns(Task.CompletedTask);

            _service = new TokenService(_configMock.Object, _tokenStoreMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task GenerateTokenAsync_ReturnsNonEmptyAccessToken()
        {
            // Arrange — user is set up in constructor

            // Act
            var result = await _service.GenerateTokenAsync(_testUser);

            // Assert
            Assert.NotNull(result);
            Assert.False(string.IsNullOrWhiteSpace(result.AccessToken));
        }

        [Fact]
        public async Task GenerateTokenAsync_ReturnsNonEmptyJti()
        {
            // Arrange
            // Act
            var result = await _service.GenerateTokenAsync(_testUser);

            // Assert
            Assert.False(string.IsNullOrWhiteSpace(result.Jti));
        }

        [Fact]
        public async Task GenerateTokenAsync_ReturnsBearerTokenType()
        {
            // Arrange
            // Act
            var result = await _service.GenerateTokenAsync(_testUser);

            // Assert
            Assert.Equal("bearer", result.TokenType);
        }

        [Fact]
        public async Task GenerateTokenAsync_ReturnsPositiveExpiresIn()
        {
            // Arrange
            // Act
            var result = await _service.GenerateTokenAsync(_testUser);

            // Assert
            Assert.True(result.ExpiresIn > 0);
        }

        [Fact]
        public async Task GenerateTokenAsync_AddsJtiToTokenStore()
        {
            // Arrange
            // Act
            var result = await _service.GenerateTokenAsync(_testUser);

            // Assert
            _tokenStoreMock.Verify(s => s.AddTokenAsync(result.Jti), Times.Once);
        }

        [Fact]
        public async Task GenerateTokenAsync_EachCallProducesUniqueJti()
        {
            // Arrange
            // Act
            var first = await _service.GenerateTokenAsync(_testUser);
            var second = await _service.GenerateTokenAsync(_testUser);

            // Assert
            Assert.NotEqual(first.Jti, second.Jti);
        }

        [Fact]
        public async Task RevokeTokenAsync_CallsTokenStore()
        {
            // Arrange
            var jti = Guid.NewGuid().ToString();

            // Act
            await _service.RevokeTokenAsync(jti);

            // Assert
            _tokenStoreMock.Verify(s => s.RevokeTokenAsync(jti), Times.Once);
        }

        [Fact]
        public async Task IsTokenValidAsync_WhenStoreReturnsTrue_ReturnsTrue()
        {
            // Arrange
            var jti = Guid.NewGuid().ToString();
            _tokenStoreMock.Setup(s => s.IsValidAsync(jti)).ReturnsAsync(true);

            // Act
            var result = await _service.IsTokenValidAsync(jti);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task IsTokenValidAsync_WhenStoreReturnsFalse_ReturnsFalse()
        {
            // Arrange
            var jti = Guid.NewGuid().ToString();
            _tokenStoreMock.Setup(s => s.IsValidAsync(jti)).ReturnsAsync(false);

            // Act
            var result = await _service.IsTokenValidAsync(jti);

            // Assert
            Assert.False(result);
        }
    }
}
