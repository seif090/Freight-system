using FreightSystem.Application.Interfaces;
using FreightSystem.Core.Entities;
using FreightSystem.Infrastructure.Persistence;
using FreightSystem.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Cryptography;
using System.Text;

namespace FreightSystem.Infrastructure.Persistence
{
    public static class DbInitializer
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<FreightDbContext>();
            var userRepo = scope.ServiceProvider.GetRequiredService<IUserRepository>();

            await context.Database.MigrateAsync();

            // Ensure roles exist via EF seed and migration.
            if (!context.Roles.Any())
            {
                context.Roles.AddRange(
                    new Role { Name = "Admin" },
                    new Role { Name = "Operation" },
                    new Role { Name = "Sales" },
                    new Role { Name = "Accountant" }
                );
                await context.SaveChangesAsync();
            }

            // Seed default users only once
            if (!context.Users.Any())
            {
                await AddDefaultUser(userRepo, "admin", "Admin123!", new[] { "Admin" });
                await AddDefaultUser(userRepo, "operation", "Op123!", new[] { "Operation" });
                await AddDefaultUser(userRepo, "sales", "Sales123!", new[] { "Sales" });
                await AddDefaultUser(userRepo, "accountant", "Ac123!", new[] { "Accountant" });
            }

            // Seed demo data for clients (shipments, delays, clusters)
            await SeedDemoDataAsync(context);
        }

        private static async Task SeedDemoDataAsync(FreightDbContext context)
        {
            if (context.Shipments.Any())
                return;

            var now = DateTime.UtcNow;

            if (!context.Customers.Any())
            {
                context.Customers.AddRange(
                    new Customer { Name = "Global Logistics Co", Email = "contact@globallogistics.com", Phone = "+12125550101", Address = "1 Expo Blvd, New York, NY", CreditLimit = 250000, Balance = 45000, CreatedAt = now.AddMonths(-6) },
                    new Customer { Name = "Cairo Imports", Email = "sales@cairoimports.eg", Phone = "+201000123456", Address = "Cairo Gate, Egypt", CreditLimit = 150000, Balance = 35000, CreatedAt = now.AddMonths(-4) }
                );
            }

            if (!context.Suppliers.Any())
            {
                context.Suppliers.AddRange(
                    new Supplier { Name = "Fairfield Shipping", CreatedAt = now.AddYears(-2) },
                    new Supplier { Name = "Delta Air Cargo", CreatedAt = now.AddYears(-1) }
                );
            }

            await context.SaveChangesAsync();

            var customer1 = context.Customers.First(c => c.Name.Contains("Global Logistics"));
            var customer2 = context.Customers.First(c => c.Name.Contains("Cairo Imports"));
            var supplier1 = context.Suppliers.First(s => s.Name.Contains("Fairfield"));

            var shipments = new[]
            {
                new Shipment
                {
                    TrackingNumber = "GLC-1001",
                    Type = ShipmentType.Import,
                    Mode = TransportMode.Sea,
                    Status = ShipmentStatus.InTransit,
                    Priority = ShipmentPriority.High,
                    PortOfLoading = "Port of Shanghai",
                    PortOfDischarge = "Port of New York",
                    ETD = now.AddDays(-12),
                    ETA = now.AddDays(-2),
                    ContainerType = ContainerType.FCL,
                    VesselOrFlightNumber = "VSL-9001",
                    OriginLatitude = 31.2304,
                    OriginLongitude = 121.4737,
                    DestinationLatitude = 40.7128,
                    DestinationLongitude = -74.0060,
                    CurrentLatitude = 36.0,
                    CurrentLongitude = -10.0,
                    CustomerId = customer1.Id,
                    SupplierId = supplier1.Id,
                    TenantId = "default",
                    CreatedAt = now.AddDays(-15)
                },
                new Shipment
                {
                    TrackingNumber = "CAI-3002",
                    Type = ShipmentType.Export,
                    Mode = TransportMode.Air,
                    Status = ShipmentStatus.Delivered,
                    Priority = ShipmentPriority.Normal,
                    PortOfLoading = "Cairo Intl Airport",
                    PortOfDischarge = "Paris CDG",
                    ETD = now.AddDays(-10),
                    ETA = now.AddDays(-7),
                    ContainerType = ContainerType.None,
                    VesselOrFlightNumber = "DLT-428",
                    OriginLatitude = 30.0444,
                    OriginLongitude = 31.2357,
                    DestinationLatitude = 49.0097,
                    DestinationLongitude = 2.5479,
                    CurrentLatitude = 49.0097,
                    CurrentLongitude = 2.5479,
                    CustomerId = customer2.Id,
                    SupplierId = supplier1.Id,
                    TenantId = "default",
                    CreatedAt = now.AddDays(-12),
                    UpdatedAt = now.AddDays(-6)
                },
                new Shipment
                {
                    TrackingNumber = "GLC-1103",
                    Type = ShipmentType.Domestic,
                    Mode = TransportMode.Land,
                    Status = ShipmentStatus.Pending,
                    Priority = ShipmentPriority.Critical,
                    PortOfLoading = "Los Angeles Warehouse",
                    PortOfDischarge = "Dallas Hub",
                    ETD = now.AddDays(1),
                    ETA = now.AddDays(4),
                    ContainerType = ContainerType.TwentyFt,
                    VesselOrFlightNumber = "TRK-455",
                    OriginLatitude = 34.0522,
                    OriginLongitude = -118.2437,
                    DestinationLatitude = 32.7767,
                    DestinationLongitude = -96.7970,
                    CurrentLatitude = 34.0522,
                    CurrentLongitude = -118.2437,
                    CustomerId = customer1.Id,
                    SupplierId = supplier1.Id,
                    TenantId = "default",
                    CreatedAt = now.AddDays(-1)
                }
            };

            context.Shipments.AddRange(shipments);
            await context.SaveChangesAsync();

            var stuckShipment = context.Shipments.First(s => s.TrackingNumber == "GLC-1001");
            var deliveredShipment = context.Shipments.First(s => s.TrackingNumber == "CAI-3002");

            context.DelayHistories.AddRange(
                new DelayHistory
                {
                    ShipmentId = stuckShipment.Id,
                    ETD = stuckShipment.ETD,
                    ETA = stuckShipment.ETA,
                    ActualDeparture = stuckShipment.ETD ?? now.AddDays(-12),
                    ActualArrival = now,
                    DurationHours = (now - (stuckShipment.ETD ?? now)).TotalHours,
                    DelayMinutes = (now - (stuckShipment.ETA ?? now)).TotalMinutes,
                    Status = stuckShipment.Status.ToString(),
                    TenantId = stuckShipment.TenantId,
                    CreatedAt = now,
                    RecordDate = now.Date
                },
                new DelayHistory
                {
                    ShipmentId = deliveredShipment.Id,
                    ETD = deliveredShipment.ETD,
                    ETA = deliveredShipment.ETA,
                    ActualDeparture = deliveredShipment.ETD ?? now.AddDays(-10),
                    ActualArrival = (deliveredShipment.ETA ?? now).AddHours(8),
                    DurationHours = 80,
                    DelayMinutes = 480,
                    Status = deliveredShipment.Status.ToString(),
                    TenantId = deliveredShipment.TenantId,
                    CreatedAt = now.AddDays(-1),
                    RecordDate = now.AddDays(-1).Date
                }
            );

            context.DelayAnomalyClusterHistories.AddRange(
                new DelayAnomalyClusterHistory
                {
                    WeekStarting = now.AddDays(-14).Date,
                    ThresholdMinutes = 30,
                    TotalClusters = 5,
                    TotalMatches = 12,
                    AvgDelayMinutes = 58,
                    CreatedAt = now.AddDays(-7)
                },
                new DelayAnomalyClusterHistory
                {
                    WeekStarting = now.AddDays(-7).Date,
                    ThresholdMinutes = 30,
                    TotalClusters = 3,
                    TotalMatches = 7,
                    AvgDelayMinutes = 40,
                    CreatedAt = now
                }
            );

            await context.SaveChangesAsync();
        }

        private static async Task AddDefaultUser(IUserRepository userRepo, string username, string password, IEnumerable<string> roles)
        {
            var existing = await userRepo.GetByUsernameAsync(username);
            if (existing != null) return;

            var user = new User
            {
                Username = username,
                PasswordHash = HashPassword(password),
                IsActive = true
            };
            await userRepo.AddUserAsync(user, roles);
        }

        private static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        public static bool VerifyPassword(string plain, string hash)
        {
            return HashPassword(plain) == hash;
        }
    }
}
