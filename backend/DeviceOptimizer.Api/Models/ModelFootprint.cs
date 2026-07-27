namespace DeviceOptimizer.Api.Models
{
    public class ModelFootprint
    {
        public int Id { get; set; }
        public string Model { get; set; } = string.Empty;
        public double ManufacturingKgCo2e { get; set; }
        public double UsePhaseKgCo2ePerYear { get; set; }
    }
}
