namespace ToDoApp.API.Middleware
{
    public class CachingMiddleware
    {
        private readonly RequestDelegate _next;
        private static readonly Dictionary<string, string> _cache = new();

        public CachingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var cacheKey = context.Request.Path.ToString();

            if (_cache.ContainsKey(cacheKey))
            {
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(_cache[cacheKey]);
                return;
            }

            var originalResponseBody = context.Response.Body;

            using (var memoryStream = new MemoryStream())
            {
                context.Response.Body = memoryStream;
                await _next(context);

                memoryStream.Seek(0, SeekOrigin.Begin);
                var responseBody = new StreamReader(memoryStream).ReadToEnd();
                _cache[cacheKey] = responseBody;

                await memoryStream.CopyToAsync(originalResponseBody);
            }
        }
    }
}
