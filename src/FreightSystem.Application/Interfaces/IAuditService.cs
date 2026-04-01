using FreightSystem.Core.Entities;

namespace FreightSystem.Application.Interfaces
{
    public interface IAuditService
    {
        Task LogAsync(AuditLog entry);
    }
}
