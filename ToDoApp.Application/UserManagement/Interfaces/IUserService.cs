using ToDoApp.Domain.Entities.User;

namespace ToDoApp.Application.UserManagement.Interfaces
{
    public interface IUserService
    {
        Task<User> CreateUserAsync(User user);
        Task<IEnumerable<User>> GetUsersAsync();
        Task<User?> GetUserByIdAsync(Guid id);
        Task<User?> UpdateUserAsync(Guid id, User user);
        Task<bool> DeleteUserAsync(Guid id);
        Task<bool> IsEmailUniqueAsync(string email);
    }
}
