using FreightSystem.Application.Interfaces;

namespace FreightSystem.Api.Middlewares
{
    public class RateLimitMiddleware
    {
        private readonly RequestDelegate _next;

        public RateLimitMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, IRateLimitService rateLimitService, IApiKeyManager apiKeyManager)
        {
            var key = context.Request.Headers.ContainsKey("X-Api-Key") ? context.Request.Headers["X-Api-Key"].ToString() : context.Connection.RemoteIpAddress?.ToString();

            if (string.IsNullOrWhiteSpace(key) || !apiKeyManager.ValidateKey(context.Request.Headers["X-Api-Key"]))
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("API Key missing or invalid");
                return;
            }

            if (!rateLimitService.AllowRequest(key))
            {
                context.Response.StatusCode = 429;
                await context.Response.WriteAsync("Rate limit exceeded");
                return;
            }

            await _next(context);
        }
    }

    public static class RateLimitMiddlewareExtensions
    {
        public static IApplicationBuilder UseRateLimit(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RateLimitMiddleware>();
        }
    }
}
