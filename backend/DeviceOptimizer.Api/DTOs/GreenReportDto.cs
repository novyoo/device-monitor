namespace DeviceOptimizer.Api.DTOs
{
    public class GreenTenantSummaryDto
    {
        public string TenantName { get; set; } = string.Empty;
        public int DeviceCount { get; set; }
        public double AvoidedCo2Kg { get; set; }
        public double TreesEquivalent { get; set; }
        public double CarKmEquivalent { get; set; }
    }

    public class GreenReportDto
    {
        public double FleetAvoidedCo2Kg { get; set; }
        public double FleetTreesEquivalent { get; set; }
        public double FleetCarKmEquivalent { get; set; }
        public List<GreenTenantSummaryDto> Tenants { get; set; } = new();
    }
}
