using ToDoApp.Domain.Entities.User;

namespace ToDoApp.Domain.Interfaces.UserManagment
{
    public interface IUserRepository
    {
        Task<User> CreateUserAsync(User user);
        Task<IEnumerable<User>> GetUsersAsync();
        Task<User?> GetUserByIdAsync(Guid id);
        Task<User?> UpdateUserAsync(User user);
        Task<bool> DeleteUserAsync(Guid id);
        Task<User?> GetUsersByEmailAsync(string email);
    }
}