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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

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
                entity.HasMany(x => x.Shipments).WithOne(x => x.Customer).HasForeignKey(x => x.CustomerId);
                entity.HasMany(x => x.Invoices).WithOne(x => x.Customer).HasForeignKey(x => x.CustomerId);
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
        }
    }
}
