using FreightSystem.Core.Entities;

namespace FreightSystem.Application.Interfaces
{
    public interface IShipmentRepository
    {
        Task<Shipment?> GetByIdAsync(int id);
        Task<Shipment?> GetByTrackingNumberAsync(string trackingNumber);
        Task<IEnumerable<Shipment>> GetAllAsync();
        Task<IEnumerable<Shipment>> GetAllByTenantAsync(string tenantId);
        Task<IEnumerable<Shipment>> SearchAsync(string query);
        Task AddAsync(Shipment shipment);
        Task UpdateAsync(Shipment shipment);
        Task DeleteAsync(Shipment shipment);
    }
}
