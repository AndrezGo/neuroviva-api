using Microsoft.OpenApi.Models;

namespace NeuroViva.Api.Extensions;

public static class SwaggerExtensions
{
    public static IServiceCollection AddSwaggerWithAuth(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "NeuroViva API",
                Version = "v1",
                Description = "API for the NeuroViva neurodegenerative disease management platform."
            });

            var securityScheme = new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Description = "Enter the Supabase JWT token: Bearer {token}",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Reference = new OpenApiReference
                {
                    Id = "Bearer",
                    Type = ReferenceType.SecurityScheme
                }
            };

            options.AddSecurityDefinition("Bearer", securityScheme);
            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                { securityScheme, Array.Empty<string>() }
            });
        });

        return services;
    }
}
