using Asp.Versioning;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using NeuroViva.Api.Extensions;
using NeuroViva.Api.Middleware;
using NeuroViva.Application;
using NeuroViva.Infrastructure;
using NeuroViva.Infrastructure.Persistence;
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

// ── Disease catalog seed ──────────────────────────────────────
// Inserts the six conditions the onboarding wizard exposes.
// ON CONFLICT DO NOTHING makes this idempotent on every restart.
await using (var seedScope = app.Services.CreateAsyncScope())
{
    var db = seedScope.ServiceProvider.GetRequiredService<NeuroVivaDbContext>();
    if (!await db.Diseases.AnyAsync())
    {
        await db.Database.ExecuteSqlRawAsync(
            """
            INSERT INTO disease (id, name, slug, description, category, active) VALUES
            ('d0000000-0000-0000-0000-000000000001', 'Alzheimer', 'alzheimer', NULL, 'neurodegenerative', true),
            ('d0000000-0000-0000-0000-000000000002', 'Parkinson', 'parkinson', NULL, 'neurodegenerative', true),
            ('d0000000-0000-0000-0000-000000000003', 'Demencia / DCL', 'dementia_mci', NULL, 'neurodegenerative', true),
            ('d0000000-0000-0000-0000-000000000004', 'ELA', 'als', NULL, 'neurodegenerative', true),
            ('d0000000-0000-0000-0000-000000000005', 'Huntington', 'huntington', NULL, 'neurodegenerative', true),
            ('d0000000-0000-0000-0000-000000000006', 'Otra', 'other', NULL, 'other', true)
            ON CONFLICT (slug) DO NOTHING;
            """);
    }
}

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
