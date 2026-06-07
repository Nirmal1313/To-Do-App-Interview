using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ToDoApp.Application.DTOs.TagDto;
using ToDoApp.Application.TagManagement.Interface;

namespace ToDoApp.API.Controllers.Tags
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TagController : ControllerBase
    {
        private readonly ITagService _service;

        public TagController(ITagService service)
        {
            _service = service;
        }

        private Guid GetUserId() => Guid.Parse(User.FindFirstValue("uid")!);

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] TagDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _service.CreateAsync(dto, GetUserId());
            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllAsync(GetUserId());
            return Ok(result);
        }

        [HttpPost("{tagId:guid}/task/{taskId:guid}")]
        public async Task<IActionResult> AssignToTask(Guid tagId, Guid taskId)
        {
            await _service.AssignTagToTaskAsync(tagId, taskId, GetUserId());
            return Ok();
        }

        [HttpDelete("{tagId:guid}/task/{taskId:guid}")]
        public async Task<IActionResult> RemoveFromTask(Guid tagId, Guid taskId)
        {
            await _service.RemoveTagFromTaskAsync(tagId, taskId, GetUserId());
            return NoContent();
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _service.DeleteAsync(id, GetUserId());
            return NoContent();
        }
    }
}
