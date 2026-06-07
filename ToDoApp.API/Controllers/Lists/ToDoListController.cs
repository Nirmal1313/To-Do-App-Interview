using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ToDoApp.Application.DTOs.ListDto;
using ToDoApp.Application.ListManagement.Interface;

namespace ToDoApp.API.Controllers.Lists
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ToDoListController : ControllerBase
    {
        private readonly IToDoListService _service;

        public ToDoListController(IToDoListService service)
        {
            _service = service;
        }

        private Guid GetUserId() => Guid.Parse(User.FindFirstValue("uid")!);

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ToDoListDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _service.CreateAsync(dto, GetUserId());
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _service.GetByIdAsync(id, GetUserId());
            if (result == null)
                return NotFound();
            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllAsync(GetUserId());
            return Ok(result);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] ToDoListDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _service.UpdateAsync(id, dto, GetUserId());
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
