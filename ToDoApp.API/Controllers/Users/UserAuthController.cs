using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using ToDoApp.Application.Auth.Interface;
using ToDoApp.Application.DTOs.LoginDto;
using ToDoApp.Application.DTOs.RegistrationDto;
using ToDoApp.Domain.Entities.User;
using ToDoApp.Domain.Interfaces.UserManagment;

namespace ToDoApp.API.Controllers.Users
{
    [ApiController]
    [Route("api/auth")]
    public class UserAuthController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly ITokenService _tokenService;
        private readonly PasswordHasher<User> _hasher = new();

        public UserAuthController(IUserRepository userRepository, ITokenService tokenService)
        {
            _userRepository = userRepository;
            _tokenService = tokenService;
        }

        [HttpPost("Register")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Register([FromBody] RegisterDto register)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existing = await _userRepository.GetUsersByEmailAsync(register.Email);
            if (existing != null)
                return Conflict(new { Message = "An account with that email already exists." });

            var user = new User
            {
                FirstName = register.FirstName.Trim(),
                LastName = register.LastName?.Trim() ?? string.Empty,
                Email = register.Email.Trim().ToLowerInvariant()
            };

            user.PasswordHash = _hasher.HashPassword(user, register.Password);

            await _userRepository.CreateUserAsync(user);

            return Created($"/api/users/{user.Id}", new { user.Email });
        }

        [HttpPost("Login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Login([FromBody] LoginDto login)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userRepository.GetUsersByEmailAsync(login.Email);
            if (user == null || user.IsDeleted)
                return Unauthorized(new { Message = "Account not found." });

            var result = _hasher.VerifyHashedPassword(user, user.PasswordHash, login.Password);
            if (result == PasswordVerificationResult.Failed)
                return Unauthorized();

            var token = await _tokenService.GenerateTokenAsync(user);

            return Ok(new
            {
                access_token = token.AccessToken,
                token_type = token.TokenType,
                expires_in = token.ExpiresIn,
                jti = token.Jti
            });
        }

        [Authorize]
        [HttpPost("Logout")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Logout()
        {
            var jti = User.FindFirstValue(JwtRegisteredClaimNames.Jti);
            if (string.IsNullOrEmpty(jti))
                return BadRequest(new { Message = "No token to revoke." });

            var uid = User.FindFirstValue("uid");
            if (string.IsNullOrEmpty(uid))
                return Unauthorized();

            await _tokenService.RevokeTokenAsync(jti);
            return NoContent();
        }
    }
}
