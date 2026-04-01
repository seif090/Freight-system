using FreightSystem.Application.Interfaces;

namespace FreightSystem.Infrastructure.Services
{
    public class TenantContext : ITenantContext
    {
        public string TenantId { get; set; } = "default";
    }
}
