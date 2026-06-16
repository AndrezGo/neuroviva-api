using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

namespace NeuroViva.Api.Extensions;

public static class RateLimitingExtensions
{
    public const string DefaultPolicy = "default";
    public const string AiPolicy = "ai";
    public const string WritePolicy = "write";

    public static IServiceCollection AddApiRateLimiting(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.AddFixedWindowLimiter(DefaultPolicy, o =>
            {
                o.PermitLimit = configuration.GetValue("RateLimiting:Default:PermitLimit", 100);
                o.Window = TimeSpan.FromSeconds(configuration.GetValue("RateLimiting:Default:WindowSeconds", 60));
                o.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                o.QueueLimit = 5;
            });

            options.AddFixedWindowLimiter(WritePolicy, o =>
            {
                o.PermitLimit = configuration.GetValue("RateLimiting:Write:PermitLimit", 30);
                o.Window = TimeSpan.FromSeconds(configuration.GetValue("RateLimiting:Write:WindowSeconds", 60));
                o.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                o.QueueLimit = 2;
            });

            options.AddFixedWindowLimiter(AiPolicy, o =>
            {
                o.PermitLimit = configuration.GetValue("RateLimiting:Ai:PermitLimit", 20);
                o.Window = TimeSpan.FromSeconds(configuration.GetValue("RateLimiting:Ai:WindowSeconds", 3600));
                o.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                o.QueueLimit = 1;
            });
        });

        return services;
    }
}
