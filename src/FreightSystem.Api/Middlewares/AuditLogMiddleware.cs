using FreightSystem.Application.Interfaces;
using FreightSystem.Core.Entities;

namespace FreightSystem.Api.Middlewares
{
    public class AuditLogMiddleware
    {
        private readonly RequestDelegate _next;

        public AuditLogMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, IAuditService auditService)
        {
            await _next(context);

            var userName = context.User?.Identity?.IsAuthenticated == true
                ? context.User.Identity?.Name ?? "Anonymous"
                : "Anonymous";

            var entry = new AuditLog
            {
                UserName = userName,
                Path = context.Request.Path,
                Method = context.Request.Method,
                StatusCode = context.Response.StatusCode,
                Timestamp = DateTime.UtcNow,
                Details = context.Request.QueryString.HasValue ? context.Request.QueryString.Value : null
            };

            await auditService.LogAsync(entry);
        }
    }

    public static class AuditLogMiddlewareExtensions
    {
        public static IApplicationBuilder UseAuditLog(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<AuditLogMiddleware>();
        }
    }
}
