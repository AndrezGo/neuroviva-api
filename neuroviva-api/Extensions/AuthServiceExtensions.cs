using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using NeuroViva.Application.Common.Authorization;

namespace NeuroViva.Api.Extensions;

public static class AuthServiceExtensions
{
    public static IServiceCollection AddSupabaseAuth(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var issuer = configuration["Supabase:JwtIssuer"]
            ?? throw new InvalidOperationException("Supabase:JwtIssuer is not configured.");
        var audience = configuration["Supabase:JwtAudience"] ?? "authenticated";

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                // Authority triggers OIDC discovery: {issuer}/.well-known/openid-configuration
                // Supabase exposes this endpoint — JWKS is discovered automatically
                options.Authority = issuer;
                options.Audience = audience;
                options.RequireHttpsMetadata = true;

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = issuer,
                    ValidateAudience = true,
                    ValidAudience = audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(30)
                };

                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = ctx =>
                    {
                        if (ctx.Exception is SecurityTokenExpiredException)
                            ctx.Response.Headers["Token-Expired"] = "true";
                        return Task.CompletedTask;
                    }
                };
            });

        services.AddAuthorizationBuilder()
            .AddPolicy(Policies.Authenticated, p => p.RequireAuthenticatedUser())
            .AddPolicy(Policies.DoctorOnly, p => p.RequireRole(Roles.Doctor))
            .AddPolicy(Policies.CaregiverOnly, p => p.RequireRole(Roles.Caregiver))
            .AddPolicy(Policies.PatientOnly, p => p.RequireRole(Roles.Patient))
            .AddPolicy(Policies.CaregiverOrDoctor, p => p.RequireRole(Roles.Caregiver, Roles.Doctor))
            .AddPolicy(Policies.AdminOnly, p => p.RequireRole(Roles.Admin))
            .AddPolicy(Policies.ScientificCommittee, p => p.RequireRole(Roles.ScientificCommittee, Roles.Admin));

        return services;
    }
}
