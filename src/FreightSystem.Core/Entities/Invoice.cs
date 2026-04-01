namespace FreightSystem.Core.Entities
{
    public enum InvoiceStatus { Draft, Issued, Paid, Overdue, Cancelled }

    public class Invoice
    {
        public int Id { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public int CustomerId { get; set; }
        public Customer Customer { get; set; } = null!;
        public int? ShipmentId { get; set; }
        public Shipment? Shipment { get; set; }

        public decimal Amount { get; set; }
        public decimal VAT { get; set; }
        public string Currency { get; set; } = "USD";
        public InvoiceStatus Status { get; set; } = InvoiceStatus.Draft;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? DueDate { get; set; }

        public ICollection<InvoiceItem> Items { get; set; } = new List<InvoiceItem>();
    }

    public class InvoiceItem
    {
        public int Id { get; set; }
        public int InvoiceId { get; set; }
        public Invoice Invoice { get; set; } = null!;

        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }

        public string ChargeType { get; set; } = "Freight";
    }
}
