namespace DeviceOptimizer.Api.DTOs
{
    public class AlertDto
    {
        public int Id { get; set; }
        public int DeviceId { get; set; }
        public string Model { get; set; } = string.Empty;
        public string SerialNumber { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
