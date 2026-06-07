using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace ToDoApp.API.Middleware
{
    public class LoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<LoggingMiddleware> _logger;
        private const string CorrelationIdHeaderKey = "X-Correlation-ID";

        public LoggingMiddleware(RequestDelegate next, ILogger<LoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (!context.Request.Headers.TryGetValue(CorrelationIdHeaderKey, out var correlationId))
            {
                correlationId = Guid.NewGuid().ToString();
            }

            context.Response.Headers.Append(CorrelationIdHeaderKey, correlationId);

            var stopwatch = Stopwatch.StartNew();

            _logger.LogInformation(
                "HTTP Request Started | CorrelationID: {CorrelationId} | Method: {Method} | Path: {Path}{QueryString}",
                correlationId, context.Request.Method, context.Request.Path, context.Request.QueryString);

            try
            {
                await _next(context);
            }
            finally
            {
                stopwatch.Stop();
                var elapsedMs = stopwatch.ElapsedMilliseconds;
                var statusCode = context.Response.StatusCode;

                if (statusCode >= 500)
                {
                    _logger.LogError(
                        "HTTP Request Failed | CorrelationID: {CorrelationId} | Status: {Status} | Duration: {Duration}ms",
                        correlationId, statusCode, elapsedMs);
                }
                else if (statusCode >= 400)
                {
                    _logger.LogWarning(
                        "HTTP Request Client Error | CorrelationID: {CorrelationId} | Status: {Status} | Duration: {Duration}ms",
                        correlationId, statusCode, elapsedMs);
                }
                else
                {
                    _logger.LogInformation(
                        "HTTP Request Completed | CorrelationID: {CorrelationId} | Status: {Status} | Duration: {Duration}ms",
                        correlationId, statusCode, elapsedMs);
                }
            }
        }
    }
}
