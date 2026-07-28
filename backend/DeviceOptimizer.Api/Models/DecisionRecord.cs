namespace DeviceOptimizer.Api.Models
{
    public class DecisionRecord
    {
        public int Id { get; set; }

        public int DeviceId { get; set; }
        public Device? Device { get; set; }

        public string RecommendedAction { get; set; } = string.Empty;
        public string ActualAction { get; set; } = string.Empty;

        public DateTime DecidedAt { get; set; } = DateTime.UtcNow;
    }
}
