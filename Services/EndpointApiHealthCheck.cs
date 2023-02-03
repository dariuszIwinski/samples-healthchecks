using System;
using Flurl.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Newtonsoft.Json;

namespace ApiHealthChecks.Services
{
    public class EndpointApiHealthCheck : IHealthCheck
    {
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await "https://reqres.in/api/users/2".AllowAnyHttpStatus().GetAsync();
                if (result.StatusCode == 200)
                    return HealthCheckResult.Healthy();
                else if (result.StatusCode < 500)
                    return HealthCheckResult.Degraded(JsonConvert.SerializeObject(result));
                else
                    return HealthCheckResult.Unhealthy(JsonConvert.SerializeObject(result));
            }
            catch (Exception exc)
            {
                return HealthCheckResult.Unhealthy(exc.Message);
            }
        }
    }
}
