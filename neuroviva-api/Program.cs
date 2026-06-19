using Asp.Versioning;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using NeuroViva.Api.Extensions;
using NeuroViva.Api.Middleware;
using NeuroViva.Application;
using NeuroViva.Infrastructure;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// ── Serilog ──────────────────────────────────────────────────
builder.Host.UseSerilog((ctx, lc) =>
    lc.ReadFrom.Configuration(ctx.Configuration)
      .Enrich.FromLogContext()
      .WriteTo.Console(outputTemplate:
          "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}"));

// ── Application & Infrastructure ────────────────────────────
builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration);

// ── Auth ─────────────────────────────────────────────────────
builder.Services.AddSupabaseAuth(builder.Configuration);

// ── API ──────────────────────────────────────────────────────
builder.Services.AddApiCors(builder.Configuration);
builder.Services.AddApiRateLimiting(builder.Configuration);

builder.Services
    .AddApiVersioning(options =>
    {
        options.DefaultApiVersion = new ApiVersion(1, 0);
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.ReportApiVersions = true;
    })
    .AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'VVV";
        options.SubstituteApiVersionInUrl = true;
    });

builder.Services.AddControllers();
builder.Services.AddSwaggerWithAuth();

builder.Services
    .AddHealthChecks()
    .AddNpgSql(
        builder.Configuration["Database:ConnectionString"]
            ?? throw new InvalidOperationException("Database:ConnectionString not set."),
        name: "postgres",
        failureStatus: HealthStatus.Unhealthy,
        tags: ["db", "ready"]);

// ── Build ────────────────────────────────────────────────────
var app = builder.Build();

app.UseSerilogRequestLogging(opts =>
    opts.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000}ms");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "NeuroViva API v1"));
}

app.UseCors(CorsExtensions.PolicyName);
if (!app.Environment.IsDevelopment())
    app.UseHttpsRedirection();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<UserResolutionMiddleware>();

app.MapControllers().RequireRateLimiting(RateLimitingExtensions.DefaultPolicy);

app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (ctx, report) =>
    {
        ctx.Response.ContentType = "application/json";
        var result = System.Text.Json.JsonSerializer.Serialize(new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                duration = e.Value.Duration.TotalMilliseconds
            })
        });
        await ctx.Response.WriteAsync(result);
    }
});

app.Run();
