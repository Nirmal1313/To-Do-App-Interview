using Microsoft.IdentityModel.Tokens;
using System;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ToDoApp.Application.Auth.Interface;
using ToDoApp.Application.DTOs.Auth;
using ToDoApp.Domain.Entities.User;
using Microsoft.Extensions.Logging;

namespace ToDoApp.Application.Auth.Service
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _configuration;
        private readonly ITokenStore _tokenStore;
        private readonly ILogger<TokenService> _logger;

        public TokenService(IConfiguration configuration, ITokenStore tokenStore, ILogger<TokenService> logger)
        {
            _configuration = configuration;
            _tokenStore = tokenStore;
            _logger = logger;
        }

        public async Task<TokenResponse> GenerateTokenAsync(User user)
        {
            _logger.LogInformation("Generating JWT token for user {Email}", user.Email);
            var secret = _configuration["Jwt:Key"] ?? "production";

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var jti = Guid.NewGuid().ToString();

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                new Claim("uid", user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, jti)
            };

            var expires = DateTime.UtcNow.AddHours(1);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: expires,
                signingCredentials: creds
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            await _tokenStore.AddTokenAsync(jti);

            return new TokenResponse
            {
                AccessToken = tokenString,
                TokenType = "bearer",
                ExpiresIn = 3600,
                Jti = jti
            };
        }

        public Task RevokeTokenAsync(string jti)
        {
            _logger.LogInformation("Revoking token with JTI: {Jti}", jti);
            return _tokenStore.RevokeTokenAsync(jti);
        }

        public Task<bool> IsTokenValidAsync(string jti)
        {
            _logger.LogInformation("Checking validity of token with JTI: {Jti}", jti);
            return _tokenStore.IsValidAsync(jti);
        }
    }
}