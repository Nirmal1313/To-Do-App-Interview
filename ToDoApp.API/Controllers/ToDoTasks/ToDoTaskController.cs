using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ToDoApp.Application.DTOs.TaskDto;
using ToDoApp.Application.TaskManagement.Interface;
using ToDoApp.Domain.Common.Enums;
using TaskStatus = ToDoApp.Domain.Common.Enums.TaskStatus;

namespace ToDoApp.API.Controllers.ToDoTasks
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ToDoTaskController : ControllerBase
    {
        private readonly ITaskManagementService _taskService;

        public ToDoTaskController(ITaskManagementService taskService)
        {
            _taskService = taskService;
        }

        private Guid GetUserId()
        {
            return Guid.Parse(User.FindFirstValue("uid")!);
        }

        [HttpPost]
        public async Task<IActionResult> CreateTask([FromBody] ToDoTaskDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();

                return BadRequest(new { message = string.Join(" ", errors) });
            }

            var userId = GetUserId();

            var result = await _taskService.AddTaskAsync(dto, userId);

            return CreatedAtAction(nameof(GetTaskById), new { id = result.Id }, result);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetTaskById(Guid id)
        {
            var userId = GetUserId();

            var task = await _taskService.GetTaskByIdAsync(id, userId);

            if (task == null)
                return NotFound();

            return Ok(task);
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAll()
        {
            var userId = GetUserId();

            var tasks = await _taskService.GetAllAsync(userId);

            return Ok(tasks);
        }

        [HttpGet("status")]
        public async Task<IActionResult> GetByStatus([FromQuery] TaskStatus status)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = GetUserId();

            var tasks = await _taskService.GetTasksByStatusAsync(userId, status);

            return Ok(tasks);
        }

        [HttpGet("priority")]
        public async Task<IActionResult> GetByPriority([FromQuery] TaskPriority priority)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var userId = GetUserId();

            var tasks = await _taskService.GetTasksByPriorityAsync(userId, priority);

            return Ok(tasks);
        }

        [HttpGet("title")]
        public async Task<IActionResult> GetByTitle([FromQuery] string title)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = GetUserId();

            var tasks = await _taskService.GetTaskByTitleAsync(title, userId);

            return Ok(tasks);
        }

        [HttpGet("list/{listId:guid}")]
        public async Task<IActionResult> GetByList(Guid listId)
        {
            var userId = GetUserId();
            var tasks = await _taskService.GetTasksByListIdAsync(listId, userId);
            return Ok(tasks);
        }

        [HttpPut("{id:guid}/tags")]
        public async Task<IActionResult> ReplaceTaskTags(Guid id, [FromBody] List<Guid> tagIds)
        {
            var userId = GetUserId();
            await _taskService.ReplaceTaskTagsAsync(id, tagIds, userId);
            return NoContent();
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateTask(Guid id, [FromBody] ToDoTaskDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();

                return BadRequest(new { message = string.Join(" ", errors) });
            }

            var userId = GetUserId();

            var result = await _taskService.UpdateTaskAsync(id, dto, userId);

            if (result == null)
                return NotFound();

            return Ok(result);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteTask(Guid id)
        {
            var userId = GetUserId();

            var result = await _taskService.DeleteTaskAsync(id, userId);

            if (!result)
                return NotFound();

            return NoContent();
        }
    }
}