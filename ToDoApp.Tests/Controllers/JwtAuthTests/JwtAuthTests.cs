using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using ToDoApp.Application.DTOs.Auth;
using ToDoApp.Application.DTOs.LoginDto;
using ToDoApp.Application.DTOs.RegistrationDto;
using Xunit;

namespace ToDoApp.Tests.Controllers.JwtAuthTests
{
    public class JwtAuthTests : IClassFixture<JwtWebApplicationFactory<Program>>
    {
        private readonly JwtWebApplicationFactory<Program> _factory;

        public JwtAuthTests(JwtWebApplicationFactory<Program> factory)
        {
            _factory = factory;
        }

        private static RegisterDto ValidRegister(string? email = null, string? password = null)
        {
            var pw = password ?? "TestPass@1234";
            return new RegisterDto
            {
                FirstName = "Test",
                LastName = "User",
                Email = email ?? $"user_{Guid.NewGuid():N}@test.com",
                Password = pw,
                ConfirmPassword = pw,
            };
        }

        private static async Task<string> LoginAndGetTokenAsync(HttpClient client)
        {
            var loginResponse = await client.PostAsJsonAsync("/api/auth/Login", new LoginDto
            {
                Email = JwtWebApplicationFactory<Program>.SeededEmail,
                Password = JwtWebApplicationFactory<Program>.SeededPassword
            });
            var loginBody = await loginResponse.Content.ReadAsStringAsync();
            loginResponse.StatusCode.Should().Be(HttpStatusCode.OK, because: loginBody);

            var loginResult = await loginResponse.Content.ReadFromJsonAsync<TokenResponse>();
            return loginResult!.AccessToken;
        }

        [Fact]
        public async Task Register_WithNewEmail_Returns201Created()
        {
            // Arrange
            var client = _factory.CreateClient();
            var dto = ValidRegister();

            // Act
            var response = await client.PostAsJsonAsync("/api/auth/Register", dto);
            var body = await response.Content.ReadAsStringAsync();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created, because: body);
        }

        [Fact]
        public async Task Register_WithDuplicateEmail_Returns409Conflict()
        {
            // Arrange
            var client = _factory.CreateClient();
            var dto = ValidRegister(email: JwtWebApplicationFactory<Program>.SeededEmail);

            // Act
            var response = await client.PostAsJsonAsync("/api/auth/Register", dto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        }

        [Fact]
        public async Task Register_ResponseBodyContainsEmail()
        {
            // Arrange
            var client = _factory.CreateClient();
            var dto = ValidRegister();

            // Act
            var response = await client.PostAsJsonAsync("/api/auth/Register", dto);
            var body = await response.Content.ReadAsStringAsync();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created, because: body);
            body.Should().Contain(dto.Email.ToLowerInvariant());
        }

        [Fact]
        public async Task Login_WithCorrectCredentials_ReturnsToken()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.PostAsJsonAsync("/api/auth/Login", new LoginDto
            {
                Email = JwtWebApplicationFactory<Program>.SeededEmail,
                Password = JwtWebApplicationFactory<Program>.SeededPassword
            });
            var body = await response.Content.ReadAsStringAsync();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK, because: body);
            var result = await response.Content.ReadFromJsonAsync<TokenResponse>();
            result.Should().NotBeNull();
            result!.AccessToken.Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public async Task Login_WithWrongPassword_Returns401Unauthorized()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.PostAsJsonAsync("/api/auth/Login", new LoginDto
            {
                Email = JwtWebApplicationFactory<Program>.SeededEmail,
                Password = "WrongPassword!"
            });

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Login_WithUnknownEmail_Returns401Unauthorized()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.PostAsJsonAsync("/api/auth/Login", new LoginDto
            {
                Email = "nobody@nowhere.com",
                Password = "anything"
            });

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task ProtectedEndpoint_WithoutToken_Returns401Unauthorized()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("/api/todotask/all");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task ProtectedEndpoint_WithValidToken_Returns200Ok()
        {
            // Arrange
            var client = _factory.CreateClient();
            var token = await LoginAndGetTokenAsync(client);
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await client.GetAsync("/api/todotask/all");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task ProtectedEndpoint_WithGarbageToken_Returns401Unauthorized()
        {
            // Arrange
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "this.is.not.valid");

            // Act
            var response = await client.GetAsync("/api/todotask/all");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Logout_WithValidToken_Returns204NoContent()
        {
            // Arrange
            var client = _factory.CreateClient();
            var token = await LoginAndGetTokenAsync(client);
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await client.PostAsync("/api/auth/Logout", null);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task Logout_WithoutToken_Returns401Unauthorized()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.PostAsync("/api/auth/Logout", null);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }
    }
}