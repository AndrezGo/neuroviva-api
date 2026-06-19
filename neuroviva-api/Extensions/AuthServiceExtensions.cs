using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using NeuroViva.Application.Common.Authorization;
using NeuroViva.Application.Features.Users.Queries;

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
                    },

                    OnTokenValidated = async ctx =>
                    {
                        var sub = ctx.Principal?.FindFirstValue(ClaimTypes.NameIdentifier)
                                  ?? ctx.Principal?.FindFirstValue("sub");

                        if (!Guid.TryParse(sub, out var authUserId)) return;

                        var repo = ctx.HttpContext.RequestServices
                            .GetRequiredService<IUserReadRepository>();

                        var data = await repo.GetClaimsByAuthUserIdAsync(
                            authUserId, ctx.HttpContext.RequestAborted);

                        if (data is null) return;

                        var identity = ctx.Principal?.Identity as ClaimsIdentity;
                        if (identity is null) return;

                        identity.AddClaim(new Claim(ClaimNames.InternalUserId, data.InternalUserId.ToString()));
                        identity.AddClaim(new Claim(ClaimNames.TenantId, data.TenantId.ToString()));
                        foreach (var role in data.Roles)
                            identity.AddClaim(new Claim(ClaimTypes.Role, role));
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
