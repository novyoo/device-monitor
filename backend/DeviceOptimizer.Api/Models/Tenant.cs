namespace DeviceOptimizer.Api.Models
{
    public class Tenant
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public List<Device> Devices { get; set; } = new();
    }
}
