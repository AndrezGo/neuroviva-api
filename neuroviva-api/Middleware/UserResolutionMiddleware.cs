using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using NeuroViva.Application.Common.Authorization;
using NeuroViva.Infrastructure.Persistence;

namespace NeuroViva.Api.Middleware;

public sealed class UserResolutionMiddleware
{
    private readonly RequestDelegate _next;

    public UserResolutionMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context, NeuroVivaDbContext db)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var sub = context.User.FindFirstValue(ClaimTypes.NameIdentifier)
                   ?? context.User.FindFirstValue(ClaimNames.Sub);

            if (Guid.TryParse(sub, out var authUserId))
            {
                var user = await db.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.AuthUserId == authUserId);

                if (user is not null)
                {
                    var roleNames = await db.UserRoles
                        .AsNoTracking()
                        .Where(ur => ur.UserId == user.Id)
                        .Join(db.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => r.Name)
                        .ToListAsync();

                    var identity = (ClaimsIdentity)context.User.Identity!;
                    identity.AddClaim(new Claim(ClaimNames.InternalUserId, user.Id.ToString()));
                    identity.AddClaim(new Claim(ClaimNames.TenantId, user.TenantId.ToString()));

                    foreach (var role in roleNames)
                        identity.AddClaim(new Claim(ClaimTypes.Role, role));

                    context.Items["UserId"] = user.Id;
                    context.Items["TenantId"] = user.TenantId;
                }
            }
        }

        await _next(context);
    }
}
