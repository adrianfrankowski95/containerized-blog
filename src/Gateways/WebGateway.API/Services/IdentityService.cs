using System.Security.Claims;

namespace Blog.Gateways.WebGateway.API.Models;

public class IdentityService : IIdentityService
{
    public IdentityService()
    {
    }

    public bool TryCreateUserIdentityFromAccessToken(string accessToken, out UserIdentity userIdentity)
    {
        userIdentity = null;

        var accessTokenOptions = _jwtOptions.AccessToken;

        var tokenValidationParameters = new TokenValidationParameters()
        {
            RequireSignedTokens = true,
            RequireExpirationTime = true,
            ValidateLifetime = false,
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = accessTokenOptions.Issuer,
            ValidAudience = accessTokenOptions.Audiences.AuthService,
            ValidAlgorithms = new[] { _audienceSigningKey.Get().Algorithm },
            IssuerSigningKey = _audienceSigningKey.Get().Key,
            ClockSkew = TimeSpan.Zero
        };

        var tokenHandler = new JwtSecurityTokenHandler()
        {
            MapInboundClaims = false
        };

        ClaimsPrincipal claimsPrincipal;
        try
        {
            claimsPrincipal = tokenHandler.ValidateToken(accessToken, tokenValidationParameters, out _);
        }
        catch
        {
            return false;
        }

        return TryCreateUserIdentityFromClaimsPrincipal(claimsPrincipal, out userIdentity);
    }

    public static bool TryCreateUserIdentityFromClaimsPrincipal(ClaimsPrincipal claimsPrincipal, out UserIdentity userIdentity)
    {
        userIdentity = null;

        try
        {
            if (!claimsPrincipal.Identity.IsAuthenticated)
                return false;

            userIdentity = new UserIdentity(
                    Guid.Parse(claimsPrincipal.FindFirstValue(UserClaimTypes.Id)),
                    claimsPrincipal.FindFirstValue(UserClaimTypes.Email),
                    claimsPrincipal.FindFirstValue(UserClaimTypes.Name),
                    claimsPrincipal.FindFirstValue(UserClaimTypes.Role),
                    bool.Parse(claimsPrincipal.FindFirstValue(UserClaimTypes.IsPersistent)));
        }
        catch
        {
            return false;
        }

        return true;
    }

}
