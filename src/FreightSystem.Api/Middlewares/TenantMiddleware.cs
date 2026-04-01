using FreightSystem.Application.Interfaces;

namespace FreightSystem.Api.Middlewares
{
    public class TenantMiddleware
    {
        private readonly RequestDelegate _next;

        public TenantMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, ITenantContext tenantContext)
        {
            if (context.Request.Headers.TryGetValue("X-Tenant-ID", out var tenantIdValues))
            {
                tenantContext.TenantId = tenantIdValues.FirstOrDefault() ?? "default";
            }

            await _next(context);
        }
    }

    public static class TenantMiddlewareExtensions
    {
        public static IApplicationBuilder UseTenant(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<TenantMiddleware>();
        }
    }
}
