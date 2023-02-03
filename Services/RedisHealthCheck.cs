using Microsoft.Extensions.Diagnostics.HealthChecks;
using StackExchange.Redis;

namespace ApiHealthChecks.Services
{
    public class RedisHealthCheck : IHealthCheck
    {
        private readonly IConnectionMultiplexer connectionMultiplexer;

        public RedisHealthCheck(IConnectionMultiplexer connectionMultiplexer)
        {
            this.connectionMultiplexer = connectionMultiplexer;
        }

        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                var database = connectionMultiplexer.GetDatabase();
                database.StringGet("health");
                return Task.FromResult(HealthCheckResult.Healthy());
            }
            catch (Exception exc)
            {
                return Task.FromResult(HealthCheckResult.Unhealthy(exc.Message));
            }
        }
    }
}
