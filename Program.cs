using ApiHealthChecks.Models;
using ApiHealthChecks.Services;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Added Redis for RedisHealthCheck
builder.Services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect("localhost"));

builder.Services.AddHealthChecks()// <-- here
    .AddCheck<RedisHealthCheck>("Redis")
    .AddCheck<SqlServerDbMasterHealthCheck>("SqlServerDbMaster")
    .AddCheck<EndpointApiHealthCheck>("EndpointApiHealthCheck");

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions()  // <-- here
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
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
