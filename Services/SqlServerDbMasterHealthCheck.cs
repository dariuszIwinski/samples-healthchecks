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
