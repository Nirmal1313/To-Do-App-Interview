using ToDoApp.Application.DTOs.Auth;
using ToDoApp.Domain.Entities.User;

namespace ToDoApp.Application.Auth.Interface
{
    public interface ITokenService
    {
        Task<TokenResponse> GenerateTokenAsync(User user);
        Task RevokeTokenAsync(string jti);
        Task<bool> IsTokenValidAsync(string jti);
    }
}
