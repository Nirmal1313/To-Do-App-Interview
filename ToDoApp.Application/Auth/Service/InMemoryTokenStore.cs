using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Concurrent;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ToDoApp.Application.Auth.Interface;

namespace ToDoApp.Application.Auth.Service
{
    public class InMemoryTokenStore : ITokenStore
    {
        private readonly ConcurrentDictionary<string, byte> _store = new();
        private readonly ILogger<InMemoryTokenStore> _logger;

        public InMemoryTokenStore(ILogger<InMemoryTokenStore> logger)
        {
            _logger = logger;
        }

        public Task AddTokenAsync(string jti)
        {
            _store[jti] = 1;
            _logger.LogInformation("Token added with JTI: {Jti}", jti);
            return Task.CompletedTask;
        }

        public Task<bool> IsValidAsync(string jti)
        {
            _logger.LogInformation("Checking if token is valid with JTI: {Jti}", jti);
            return Task.FromResult(_store.ContainsKey(jti));
        }

        public Task RevokeTokenAsync(string jti)
        {
            _logger.LogInformation("Revoking token with JTI: {Jti}", jti);
            _store.TryRemove(jti, out _);
            return Task.CompletedTask;
        }

        public Task ClearAllAsync()
        {
            _logger.LogInformation("Clearing all tokens from the store");
            _store.Clear();
            return Task.CompletedTask;
        }
    }
}