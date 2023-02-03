namespace ApiHealthChecks.Models
{
    public class HealthCheckResponse
    {
        public string? State { get; set; }
        public TimeSpan TotalDuration { get; set; }
        public IEnumerable<HealthCheck>? Checks { get; set; }
    }
}
