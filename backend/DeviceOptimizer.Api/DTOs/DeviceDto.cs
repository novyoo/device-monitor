namespace DeviceOptimizer.Api.DTOs
{
    public class DeviceDto
    {
        public int Id { get; set; }
        public string TenantName { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string SerialNumber { get; set; } = string.Empty;
        public DateTime PurchaseDate { get; set; }
        public int RepairCount { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? ReturnedAt { get; set; }

        public int? HealthScore { get; set; }
        public string? HealthBand { get; set; }

        public string? Recommendation { get; set; }
        public List<string> RecommendationReasons { get; set; } = new();
    }
}
