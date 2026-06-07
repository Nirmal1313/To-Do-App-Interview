using Microsoft.EntityFrameworkCore;
using ToDoApp.Domain.Entities.Lists;
using ToDoApp.Domain.Interfaces.Lists;
using ToDoApp.Infrastructure.Data;

namespace ToDoApp.Infrastructure.Repositories.Lists
{
    public class ToDoListRepository : IToDoListRepository
    {
        private readonly AppDbContext _dbContext;

        public ToDoListRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ToDoList> CreateAsync(ToDoList list)
        {
            await _dbContext.ToDoLists.AddAsync(list);
            await _dbContext.SaveChangesAsync();
            return list;
        }

        public async Task<ToDoList?> GetByIdAsync(Guid id)
        {
            return await _dbContext.ToDoLists.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        }

        public async Task<IEnumerable<ToDoList>> GetByUserIdAsync(Guid userId)
        {
            return await _dbContext.ToDoLists.AsNoTracking().Where(x => x.UserId == userId && !x.IsDeleted).ToListAsync();
        }

        public async Task<ToDoList?> UpdateAsync(ToDoList list)
        {
            var existing = await _dbContext.ToDoLists.FirstOrDefaultAsync(x => x.Id == list.Id && !x.IsDeleted);

            if (existing == null)
                return null;

            existing.Name = list.Name;
            existing.Color = list.Color;

            await _dbContext.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteAsync(Guid id, Guid deletedBy)
        {
            var existing = await _dbContext.ToDoLists.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

            if (existing == null)
                return false;

            existing.IsDeleted = true;
            existing.UpdatedBy = deletedBy;
            existing.LastUpdatedDate = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();
            return true;
        }
    }
}
