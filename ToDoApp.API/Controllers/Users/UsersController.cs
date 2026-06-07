using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ToDoApp.Application.UserManagement.Interfaces;
using ToDoApp.Domain.Entities.User;

namespace ToDoApp.API.Controllers.Users
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            var users = await _userService.GetUsersAsync();
            return Ok(users);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUserById(Guid id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
                return NotFound();
            return Ok(user);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<User>> UpdateUser(Guid id, [FromBody] User user)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var updatedUser = await _userService.UpdateUserAsync(id, user);
            if (updatedUser == null)
                return NotFound();
            return Ok(updatedUser);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            var deleted = await _userService.DeleteUserAsync(id);
            if (!deleted)
                return NotFound();
            return NoContent();
        }

        [HttpGet("email/{email}")]
        public async Task<ActionResult<bool>> IsEmailUnique(string email)
        {
            var isUnique = await _userService.IsEmailUniqueAsync(email);
            return Ok(new { email = email, isUnique = isUnique });
        }
    }
}
