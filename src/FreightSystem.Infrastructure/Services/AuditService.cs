using FreightSystem.Application.Interfaces;
using FreightSystem.Core.Entities;
using FreightSystem.Infrastructure.Persistence;

namespace FreightSystem.Infrastructure.Services
{
    public class AuditService : IAuditService
    {
        private readonly FreightDbContext _dbContext;

        public AuditService(FreightDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task LogAsync(AuditLog entry)
        {
            _dbContext.AuditLogs.Add(entry);
            await _dbContext.SaveChangesAsync();
        }
    }
}
