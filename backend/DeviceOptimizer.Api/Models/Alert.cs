namespace DeviceOptimizer.Api.Models
{
    public class Alert
    {
        public int Id { get; set; }

        public int DeviceId { get; set; }
        public Device? Device { get; set; }

        public string Message { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool Dismissed { get; set; }
    }
}
