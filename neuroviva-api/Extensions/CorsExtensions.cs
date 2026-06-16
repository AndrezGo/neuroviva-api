namespace NeuroViva.Api.Extensions;

public static class CorsExtensions
{
    public const string PolicyName = "NeuroVivaCors";

    public static IServiceCollection AddApiCors(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var origins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
                   ?? Array.Empty<string>();

        services.AddCors(options =>
        {
            options.AddPolicy(PolicyName, policy =>
            {
                if (origins.Length > 0)
                    policy.WithOrigins(origins);
                else
                    policy.AllowAnyOrigin();

                policy
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .WithExposedHeaders("Token-Expired");
            });
        });

        return services;
    }
}
