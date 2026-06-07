
namespace ToDoApp.Application.Auth.Interface
{
    public interface ITokenStore
    {
        Task AddTokenAsync(string jti);
        Task<bool> IsValidAsync(string jti);
        Task RevokeTokenAsync(string jti);
        Task ClearAllAsync();
    }
}
