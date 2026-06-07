using Microsoft.Extensions.Logging;
using ToDoApp.Application.UserManagement.Interfaces;
using ToDoApp.Domain.Entities.User;
using ToDoApp.Domain.Interfaces.UserManagment;

namespace ToDoApp.Application.UserManagement.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<UserService> _logger;

        public UserService(IUserRepository userRepository, ILogger<UserService> logger)
        {
            _userRepository = userRepository;
            _logger = logger;
        }

        public async Task<User> CreateUserAsync(User user)
        {
            _logger.LogInformation("Creating user with email: {Email}", user.Email);
            return await _userRepository.CreateUserAsync(user);
        }

        public async Task<IEnumerable<User>> GetUsersAsync()
        {
            _logger.LogInformation("Fetching all users");
            return await _userRepository.GetUsersAsync();
        }

        public async Task<User?> GetUserByIdAsync(Guid id)
        {
            _logger.LogInformation("Fetching user with ID: {UserId}", id);
            return await _userRepository.GetUserByIdAsync(id);
        }

        public async Task<User?> UpdateUserAsync(Guid id, User user)
        {
            _logger.LogInformation("Updating user with ID: {UserId}", id);
            User? existingUser = await _userRepository.GetUserByIdAsync(id);

            if (existingUser == null)
            {
                _logger.LogWarning("User not found with ID: {UserId}", id);
                return null;
            }

            user.Id = existingUser.Id;
            return await _userRepository.UpdateUserAsync(user);
        }

        public async Task<bool> DeleteUserAsync(Guid id)
        {
            _logger.LogInformation("Deleting user with ID: {UserId}", id);
            return await _userRepository.DeleteUserAsync(id);
        }

        public async Task<bool> IsEmailUniqueAsync(string email)
        {
            _logger.LogInformation("Checking if email is unique: {Email}", email);
            User? user = await _userRepository.GetUsersByEmailAsync(email);
            _logger.LogInformation("Email {Email} is {Status}", email, user == null ? "unique" : "not unique");
            return user == null;
        }
    }
}
