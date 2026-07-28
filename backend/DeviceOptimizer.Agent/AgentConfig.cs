namespace DeviceOptimizer.Agent
{
    public class AgentConfig
    {
        public string ServerUrl { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
        public int IntervalMinutes { get; set; } = 60;
    }
}
