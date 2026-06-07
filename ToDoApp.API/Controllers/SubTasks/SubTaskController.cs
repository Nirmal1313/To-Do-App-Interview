using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ToDoApp.Application.DTOs.SubTaskDto;
using ToDoApp.Application.SubTaskManagement.Interface;

namespace ToDoApp.API.Controllers.SubTasks
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SubTaskController : ControllerBase
    {
        private readonly ISubTaskService _service;

        public SubTaskController(ISubTaskService service)
        {
            _service = service;
        }

        private Guid GetUserId() => Guid.Parse(User.FindFirstValue("uid")!);

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] SubTaskDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _service.CreateAsync(dto);
            return Ok(result);
        }

        [HttpGet("task/{parentTaskId:guid}")]
        public async Task<IActionResult> GetByTask(Guid parentTaskId)
        {
            var result = await _service.GetByTaskIdAsync(parentTaskId);
            return Ok(result);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] SubTaskDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _service.UpdateAsync(id, dto);
            if (result == null)
                return NotFound();
            return Ok(result);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _service.DeleteAsync(id, GetUserId());
            return NoContent();
        }
    }
}
