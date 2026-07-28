namespace DeviceOptimizer.Api.DTOs
{
    public class RegisterDeviceDto
    {
        public string Model { get; set; } = string.Empty;
        public string SerialNumber { get; set; } = string.Empty;
        public int TenantId { get; set; }
        public DateTime? PurchaseDate { get; set; }
    }

    public class RegisteredDeviceDto
    {
        public int Id { get; set; }
        public Guid ApiKey { get; set; }
    }
}
