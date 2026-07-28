namespace DeviceOptimizer.Api.DTOs
{
    public class ReturnStatsDto
    {
        public string Month { get; set; } = string.Empty;
        public int ReturnedThisMonth { get; set; }
        public int AwaitingDecision { get; set; }
        public int? AgreementPercent { get; set; }
    }
}
