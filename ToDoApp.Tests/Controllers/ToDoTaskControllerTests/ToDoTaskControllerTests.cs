using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using ToDoApp.Application.DTOs.TaskDto;
using ToDoApp.Domain.Common.Enums;
using ToDoApp.Infrastructure.Data;
using ToDoApp.Tests.TestHelpers;
using Xunit;
using DomainTaskStatus = ToDoApp.Domain.Common.Enums.TaskStatus;

namespace ToDoApp.Tests.Controllers.ToDoTaskControllerTests
{

    public class ToDoTaskApiTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly CustomWebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public ToDoTaskApiTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        private async Task<Guid> SeedTaskAsync(bool ownedByTestUser = true, string? title = null)
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var owner = ownedByTestUser ? TestAuthHandler.TestUserId : Guid.NewGuid();
            var task = TaskBuilder.Create().CreatedBy(owner).WithTitle(title ?? "Seeded Task").Build();
            db.ToDoTasks.Add(task);
            await db.SaveChangesAsync();
            return task.Id;
        }

        [Fact]
        public async Task CreateTask_WithValidBody_Returns201Created()
        {
            var dto = new ToDoTaskDto { Title = "Integration Test Task", Description = "Created in test", ListId = Guid.NewGuid() };

            var response = await _client.PostAsJsonAsync("/api/todotask?todoListId=1", dto);

            response.StatusCode.Should().Be(HttpStatusCode.Created);
        }

        [Fact]
        public async Task CreateTask_ResponseBodyContainsTitle()
        {
            var dto = new ToDoTaskDto { Title = "Title Check Task", Description = "desc", ListId = Guid.NewGuid() };

            var response = await _client.PostAsJsonAsync("/api/todotask?todoListId=1", dto);
            var result = await response.Content.ReadFromJsonAsync<ToDoTaskDto>();

            result.Should().NotBeNull();
            result!.Title.Should().Be("Title Check Task");
        }

        [Fact]
        public async Task CreateTask_ResponseHasLocationHeader()
        {
            var dto = new ToDoTaskDto { Title = "Location Header Task", Description = "desc", ListId = Guid.NewGuid() };

            var response = await _client.PostAsJsonAsync("/api/todotask?todoListId=1", dto);

            response.Headers.Location.Should().NotBeNull("CreatedAtAction must set the Location header");
        }

        [Fact]
        public async Task GetTaskById_WhenOwnedByTestUser_Returns200Ok()
        {
            var taskId = await SeedTaskAsync(ownedByTestUser: true, title: "My Task");

            var response = await _client.GetAsync($"/api/todotask/{taskId}");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetTaskById_ResponseBodyMatchesSeedTitle()
        {
            var taskId = await SeedTaskAsync(ownedByTestUser: true, title: "Specific Title");

            var result = await _client.GetFromJsonAsync<ToDoTaskDto>($"/api/todotask/{taskId}");

            result!.Title.Should().Be("Specific Title");
        }

        [Fact]
        public async Task GetTaskById_WhenTaskDoesNotExist_Returns404NotFound()
        {
            var response = await _client.GetAsync($"/api/todotask/{Guid.NewGuid()}");

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GetTaskById_WhenOwnedByDifferentUser_ReturnsNonSuccess()
        {
            var taskId = await SeedTaskAsync(ownedByTestUser: false);

            var response = await _client.GetAsync($"/api/todotask/{taskId}");

            response.IsSuccessStatusCode.Should().BeFalse(
                "a user must not be able to read another user's task");
        }

        [Fact]
        public async Task GetAll_Returns200Ok()
        {
            var response = await _client.GetAsync("/api/todotask/all");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetAll_DoesNotReturnOtherUsersTask()
        {
            await SeedTaskAsync(ownedByTestUser: false, title: "OtherUserTask");
            await SeedTaskAsync(ownedByTestUser: true, title: "MyTask");

            var tasks = await _client.GetFromJsonAsync<List<ToDoTaskDto>>("/api/todotask/all");

            tasks.Should().NotContain(t => t.Title == "OtherUserTask",
                "GetAll filters by the authenticated user's ID");
        }

        [Fact]
        public async Task GetByStatus_Returns200Ok()
        {
            var response = await _client.GetAsync("/api/todotask/status?status=Created");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetByPriority_Returns200Ok()
        {
            var response = await _client.GetAsync("/api/todotask/priority?priority=Medium");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task UpdateTask_WhenOwner_Returns200Ok()
        {
            var taskId = await SeedTaskAsync(ownedByTestUser: true);
            var update = new ToDoTaskDto { Title = "Updated Title", Description = "desc", Status = DomainTaskStatus.Created, ListId = Guid.NewGuid() };

            var response = await _client.PutAsJsonAsync($"/api/todotask/{taskId}", update);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task UpdateTask_WhenOwner_ResponseBodyHasNewTitle()
        {
            var taskId = await SeedTaskAsync(ownedByTestUser: true);
            var update = new ToDoTaskDto { Title = "New Title After Update", Description = "desc", Status = DomainTaskStatus.Created, ListId = Guid.NewGuid() };

            var response = await _client.PutAsJsonAsync($"/api/todotask/{taskId}", update);
            var result = await response.Content.ReadFromJsonAsync<ToDoTaskDto>();

            result!.Title.Should().Be("New Title After Update");
        }

        [Fact]
        public async Task UpdateTask_WhenTaskNotFound_ReturnsNonSuccess()
        {
            var update = new ToDoTaskDto { Title = "Doesn't Matter", Description = "desc", ListId = Guid.NewGuid() };
            var response = await _client.PutAsJsonAsync($"/api/todotask/{Guid.NewGuid()}", update);

            response.IsSuccessStatusCode.Should().BeFalse();
        }

        [Fact]
        public async Task DeleteTask_WhenOwner_Returns204NoContent()
        {
            var taskId = await SeedTaskAsync(ownedByTestUser: true);
            var response = await _client.DeleteAsync($"/api/todotask/{taskId}");

            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task DeleteTask_WhenOwner_TaskIsNoLongerAccessible()
        {
            var taskId = await SeedTaskAsync(ownedByTestUser: true);

            await _client.DeleteAsync($"/api/todotask/{taskId}");
            var getResponse = await _client.GetAsync($"/api/todotask/{taskId}");

            getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound,
                "after soft-delete the task must not be returned");
        }

        [Fact]
        public async Task DeleteTask_WhenTaskNotFound_ReturnsNonSuccess()
        {
            var response = await _client.DeleteAsync($"/api/todotask/{Guid.NewGuid()}");

            response.IsSuccessStatusCode.Should().BeFalse();
        }
    }
}