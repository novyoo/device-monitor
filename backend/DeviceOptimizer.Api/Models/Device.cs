namespace DeviceOptimizer.Api.Models
{
    public enum DeviceStatus
    {
        InStock,
        Rented,
        Returned,
        InRepair,
        Retired
    }

    public class Device
    {
        public int Id { get; set; }

        public int TenantId { get; set; }
        public Tenant? Tenant { get; set; }

        public string Model { get; set; } = string.Empty;
        public string SerialNumber { get; set; } = string.Empty;

        public Guid ApiKey { get; set; } = Guid.NewGuid();

        public DateTime PurchaseDate { get; set; }
        public int RepairCount { get; set; }

        public DeviceStatus Status { get; set; } = DeviceStatus.InStock;
        public DateTime? ReturnedAt { get; set; }

        public List<CheckIn> CheckIns { get; set; } = new();
    }
}
