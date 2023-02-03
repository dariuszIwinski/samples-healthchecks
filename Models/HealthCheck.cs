namespace ApiHealthChecks.Models
{
    public class HealthCheck
    {
        public string? State { get; set; }
        public string? Resource { get; set; }
        public TimeSpan Duration { get; set; }
        public string? Description { get; set; }

    }
}
