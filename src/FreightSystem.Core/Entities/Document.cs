namespace FreightSystem.Core.Entities
{
    public enum DocumentType { BillOfLading, Invoice, PackingList, CustomsDocument, Other }

    public class Document
    {
        public int Id { get; set; }
        public int ShipmentId { get; set; }
        public Shipment Shipment { get; set; } = null!;
        public DocumentType Type { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    }
}
