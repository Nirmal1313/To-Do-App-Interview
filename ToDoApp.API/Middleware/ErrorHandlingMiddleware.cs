using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace ToDoApp.API.Middleware
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlingMiddleware> _logger;

        public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception has occurred: {Message}", ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var (statusCode, title, detail) = exception switch
            {
                UnauthorizedAccessException _ => (StatusCodes.Status401Unauthorized, "Unauthorized", "Access denied."),
                KeyNotFoundException _ => (StatusCodes.Status404NotFound, "Not Found", "The requested resource could not be found."),
                BadHttpRequestException _ => (StatusCodes.Status400BadRequest, "Bad Request", "Invalid request syntax."),
                _ => (StatusCodes.Status500InternalServerError, "Server Error", "An unexpected error occurred.")
            };

            var problemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Detail = detail,
                Instance = context.Request.Path
            };

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;

            var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            var jsonResponse = JsonSerializer.Serialize(problemDetails, jsonOptions);

            await context.Response.WriteAsync(jsonResponse);
        }
    }
}
