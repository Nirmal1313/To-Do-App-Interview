using Microsoft.EntityFrameworkCore;
using ToDoApp.Domain.Entities.User;
using ToDoApp.Domain.Interfaces.UserManagment;
using ToDoApp.Infrastructure.Data;

namespace ToDoApp.Infrastructure.Repositories.UserManagement
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _dbContext;

        public UserRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<User> CreateUserAsync(User user)
        {
            user.Id = Guid.NewGuid();
            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();
            return user;
        }

        public async Task<IEnumerable<User>> GetUsersAsync()
        {
          return await _dbContext.Users.Where(x => !x.IsDeleted).ToListAsync();
        }

        public async Task<User?> GetUserByIdAsync(Guid id)
        {
            return await _dbContext.Users.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        }

        public async Task<User?> UpdateUserAsync(User user)
        {
            User? existingUser = await _dbContext.Users.FirstOrDefaultAsync(x => x.Id == user.Id && !x.IsDeleted);

            if (existingUser == null)
            {
                return null;
            }

            existingUser.FirstName = user.FirstName;
            existingUser.LastName = user.LastName;
            existingUser.Email = user.Email;

            _dbContext.Users.Update(existingUser);
            await _dbContext.SaveChangesAsync();

            return existingUser;
        }

        public async Task<bool> DeleteUserAsync(Guid id)
        {
            User? user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

            if (user == null)
                return false;

            user.IsDeleted = true;
            user.LastUpdatedDate = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();

            return true;
        }

        public async Task<User?> GetUsersByEmailAsync(string email) =>
            await _dbContext.Users.FirstOrDefaultAsync(x => x.Email == email.Trim().ToLowerInvariant() && !x.IsDeleted);
    }
}
