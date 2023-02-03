# samples-healthchecks
Sample useage of health checks in .NET API 

Add `AddHealthChecks()` to services to IServiceCollection. Then add your custom checks.
```
builder.Services.AddHealthChecks()// <-- here
                .AddCheck<RedisHealthCheck>("Redis")
                .AddCheck<SqlServerDbMasterHealthCheck>("SqlServerDbMaster")
                .AddCheck<EndpointApiHealthCheck>("EndpointApiHealthCheck");
```

Before UseEndpoints or MapControllers add `UseHealthChecks` if you want to use it under `/` path or `MapHealthChecks` if sepcified path (not allowing `/`) like `/health`.

To return general health state and states of each custom check setup HealthCheckOptions. Here with added custom classes.

```

app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions()
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";

        var response = new HealthCheckResponse()
        {
            State = report.Status.ToString(),
            Checks = report.Entries.Select(x => new HealthCheck
            {
                Resource = x.Key,
                State = x.Value.Status.ToString(),
                Description = x.Value.Description,
                Duration = x.Value.Duration
            }),
            TotalDuration = report.TotalDuration
        };

        await context.Response.WriteAsJsonAsync(response);
    },
    AllowCachingResponses = false,

});
```


A custom health check should inherit from IHealthCheck and implement it's method `CheckHealthAsync`.

*Sample for Redis*
```
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

```


*Sample for Endpoint*
```
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
```

*Sample for SqlServer without EF*
```
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace ApiHealthChecks.Services
{
    public class SqlServerDbMasterHealthCheck : IHealthCheck
    {
        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                // Just a sample - provide query to the database that you want to check
                //spt_monitor always contain a record
                const string query = "SELECT COUNT(*) FROM [master].[dbo].[spt_monitor]";

                using SqlConnection connection = new("Server=localhost;Database=master;User Id=sa;Password=tempPassw*rd123;Trust Server Certificate=true;");
                using SqlCommand command = new(query, connection);
                connection.Open();

                if ((int)command.ExecuteScalar() == 1)
                    return Task.FromResult(HealthCheckResult.Healthy());
                else
                    return Task.FromResult(HealthCheckResult.Degraded("No records in master.dbo.spt_monitor"));
            }
            catch (Exception exc)
            {
                return Task.FromResult(HealthCheckResult.Unhealthy(exc.Message));
            }
        }
    }
}

```
