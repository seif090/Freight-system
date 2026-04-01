using FreightSystem.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace FreightSystem.Infrastructure.Persistence
{
    public class FreightDbContext : DbContext
    {
        public FreightDbContext(DbContextOptions<FreightDbContext> options) : base(options)
        {
        }

        public DbSet<Shipment> Shipments => Set<Shipment>();
        public DbSet<ShipmentDetail> ShipmentDetails => Set<ShipmentDetail>();
        public DbSet<Customer> Customers => Set<Customer>();
        public DbSet<Supplier> Suppliers => Set<Supplier>();
        public DbSet<Document> Documents => Set<Document>();
        public DbSet<Invoice> Invoices => Set<Invoice>();
        public DbSet<InvoiceItem> InvoiceItems => Set<InvoiceItem>();
        public DbSet<User> Users => Set<User>();
        public DbSet<Role> Roles => Set<Role>();
        public DbSet<UserRole> UserRoles => Set<UserRole>();
        public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<AuditLog>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.Property(x => x.UserName).IsRequired();
                entity.Property(x => x.Path).IsRequired();
                entity.Property(x => x.Method).IsRequired();
                entity.Property(x => x.Timestamp).IsRequired();
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Username).IsRequired();
                entity.HasIndex(x => x.Username).IsUnique();
                entity.Property(x => x.PasswordHash).IsRequired();
                entity.HasMany(x => x.UserRoles).WithOne(x => x.User).HasForeignKey(x => x.UserId);
            });

            modelBuilder.Entity<Role>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Name).IsRequired();
                entity.HasIndex(x => x.Name).IsUnique();
                entity.HasMany(x => x.UserRoles).WithOne(x => x.Role).HasForeignKey(x => x.RoleId);
            });

            modelBuilder.Entity<UserRole>(entity =>
            {
                entity.HasKey(x => new { x.UserId, x.RoleId });
            });

            modelBuilder.Entity<Shipment>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.Property(x => x.TrackingNumber).IsRequired();
                entity.HasIndex(x => x.TrackingNumber).IsUnique();
                entity.HasMany(x => x.Details).WithOne(x => x.Shipment).HasForeignKey(x => x.ShipmentId);
                entity.HasMany(x => x.Documents).WithOne(x => x.Shipment).HasForeignKey(x => x.ShipmentId);
            });

            modelBuilder.Entity<ShipmentDetail>(entity => { entity.HasKey(x => x.Id); });

            modelBuilder.Entity<Customer>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Name).IsRequired();
                entity.HasIndex(x => x.Email).IsUnique();
                entity.HasMany(x => x.Shipments).WithOne(x => x.Customer).HasForeignKey(x => x.CustomerId);
                entity.HasMany(x => x.Invoices).WithOne(x => x.Customer).HasForeignKey(x => x.CustomerId);

                entity.HasData(
                    new Customer { Id = 1, Name = "Samah Logistics", Email = "samah@logistics.com", Phone = "+201000000001", Address = "Cairo", CreditLimit = 50000, Balance = 0, CreatedAt = DateTime.UtcNow },
                    new Customer { Id = 2, Name = "Nile Importers", Email = "contact@nile-import.com", Phone = "+201000000002", Address = "Alexandria", CreditLimit = 100000, Balance = 10000, CreatedAt = DateTime.UtcNow }
                );
            });

            modelBuilder.Entity<Supplier>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Name).IsRequired();
            });

            modelBuilder.Entity<Document>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.Property(x => x.FileName).IsRequired();
                entity.Property(x => x.FilePath).IsRequired();
            });

            modelBuilder.Entity<Invoice>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.Property(x => x.InvoiceNumber).IsRequired();
                entity.HasIndex(x => x.InvoiceNumber).IsUnique();
                entity.HasMany(x => x.Items).WithOne(x => x.Invoice).HasForeignKey(x => x.InvoiceId);
            });

            modelBuilder.Entity<InvoiceItem>(entity => { entity.HasKey(x => x.Id); });

            // Seed roles to support database-backed RBAC
            modelBuilder.Entity<Role>().HasData(
                new Role { Id = 1, Name = "Admin" },
                new Role { Id = 2, Name = "Operation" },
                new Role { Id = 3, Name = "Sales" },
                new Role { Id = 4, Name = "Accountant" }
            );

            // We intentionally do not seed users with plain passwords here; seeding is managed on startup levers via data seeder.
        }
    }
}
