using FreightSystem.Application.Interfaces;
using FreightSystem.Core.Entities;
using FreightSystem.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Net.Http;

namespace FreightSystem.Infrastructure.Services
{
    public class TelematicsService : ITelematicsService
    {
        private readonly FreightDbContext _dbContext;

        public TelematicsService(FreightDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task AddDataAsync(TelematicsData data)
        {
            _dbContext.Telematics.Add(data);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<IEnumerable<TelematicsData>> GetByShipmentAsync(int shipmentId)
        {
            return await _dbContext.Telematics
                .Where(x => x.ShipmentId == shipmentId)
                .OrderByDescending(x => x.Timestamp)
                .ToListAsync();
        }
    }

    public class TrafficService : ITrafficService
    {
        public TrafficService()
        {
        }

        public async Task<string> GetTrafficForecastAsync(double latitude, double longitude)
        {
            // This is a stub. In a real system call an external traffic/weather API.
            await Task.Delay(150);
            return $"Clear flow expected at {latitude}, {longitude} for next 30 mins.";
        }
    }

    public class AiTrainingService : IAiTrainingService
    {
        public Task<string> QueueTrainingAsync()
        {
            // Simulate queuing a training job.
            return Task.FromResult("Training queued; job-id: ai-train-" + Guid.NewGuid().ToString("N"));
        }

        public Task<string> GetTrainingStatusAsync()
        {
            return Task.FromResult("Training RUNNING (approx 22% complete)");
        }
    }
}
