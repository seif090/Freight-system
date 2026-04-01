using FreightSystem.Application.Interfaces;
using FreightSystem.Core.Entities;
using FreightSystem.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FreightSystem.Infrastructure.Repositories
{
    public class ShipmentRepository : IShipmentRepository
    {
        private readonly FreightDbContext _dbContext;

        public ShipmentRepository(FreightDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task AddAsync(Shipment shipment)
        {
            await _dbContext.Shipments.AddAsync(shipment);
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(Shipment shipment)
        {
            _dbContext.Shipments.Remove(shipment);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<IEnumerable<Shipment>> GetAllAsync()
        {
            return await _dbContext.Shipments
                .Include(x => x.Customer)
                .Include(x => x.Supplier)
                .Include(x => x.Details)
                .Include(x => x.Documents)
                .ToListAsync();
        }

        public async Task<Shipment?> GetByIdAsync(int id)
        {
            return await _dbContext.Shipments
                .Include(x => x.Customer)
                .Include(x => x.Supplier)
                .Include(x => x.Details)
                .Include(x => x.Documents)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<Shipment?> GetByTrackingNumberAsync(string trackingNumber)
        {
            return await _dbContext.Shipments
                .Include(x => x.Customer)
                .Include(x => x.Supplier)
                .Include(x => x.Details)
                .Include(x => x.Documents)
                .FirstOrDefaultAsync(x => x.TrackingNumber == trackingNumber);
        }

        public async Task UpdateAsync(Shipment shipment)
        {
            _dbContext.Shipments.Update(shipment);
            await _dbContext.SaveChangesAsync();
        }
    }
}
