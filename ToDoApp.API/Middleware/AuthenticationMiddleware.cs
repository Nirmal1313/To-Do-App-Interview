using System.IdentityModel.Tokens.Jwt;
using ToDoApp.Application.Auth.Interface;

namespace ToDoApp.API.Middleware
{
    public class AuthenticationMiddleware
    {
        private readonly RequestDelegate _next;

        public AuthenticationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, ITokenService tokenService)
        {
            var jti = context.User.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;

            if (!string.IsNullOrEmpty(jti))
            {
                var isValid = await tokenService.IsTokenValidAsync(jti);
                if (!isValid)
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsJsonAsync(new { Message = "Token has been revoked." });
                    return;
                }
            }

            await _next(context);
        }
    }
}