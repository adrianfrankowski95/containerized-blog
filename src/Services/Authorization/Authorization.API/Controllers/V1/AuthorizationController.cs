using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.RegularExpressions;
using Blog.Services.Auth.API.Config;
using Blog.Services.Auth.API.Models;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace Blog.Services.Auth.API.Controllers;

public class AuthorizationController : ControllerBase
{
    private readonly ILogger<AuthorizationController> _logger;
    private readonly TokensOptions _tokensOptions;
    private readonly AuthOptions _authOptions;

    public AuthorizationController(
        IOptions<TokensOptions> jwtOptions,
        IOptions<AuthOptions> authOptions,
        ILogger<AuthorizationController> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _tokensOptions = jwtOptions.Value ?? throw new ArgumentNullException(nameof(jwtOptions));
        _authOptions = authOptions.Value ?? throw new ArgumentNullException(nameof(authOptions));
    }

    [AllowAnonymous]
    [HttpPost("connect/authorize")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
    public async Task<IActionResult> AuthorizeAsync([FromBody, Required] UserCredentials userCredentials)
    {
        userCredentials = new("123@123.pl", "password", true);

        var request = HttpContext.GetOpenIddictServerRequest() ??
            throw new InvalidOperationException("Could not retrieve the Open ID Connect request");

        var principal = HttpContext.AuthenticateAsync("xd");
    }

    [AllowAnonymous]
    [HttpGet("refresh")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
    public async Task<IActionResult> RefreshAsync()
    {
        string? accessToken = Request.GetTokenFromCookie(_tokensOptions.AccessToken);
        string? refreshToken = Request.GetTokenFromCookie(_tokensOptions.RefreshToken);

        //checking if tokens exist in cookies
        if (string.IsNullOrWhiteSpace(accessToken) || string.IsNullOrWhiteSpace(refreshToken))
            return Unauthorized($"You are not authorized to perform this operation");

        //extracting user identity from expired access token
        if (!_tokenManager.TryGetUserIdentityFromAccessToken(accessToken, out UserIdentity userIdentity))
            return Unauthorized($"You are not authorized to perform this operation");

        //validating refresh token and checking with the database
        //active refresh tokens will be revoked if provided one has already been used in the past
        if (!await _tokenManager.ValidateRefreshTokenAsync(userIdentity.UserId, refreshToken).ConfigureAwait(false))
            return Unauthorized($"You are not authorized to perform this operation");

        try
        {
            await _tokenManager.RevokeRefreshTokenAsync(userIdentity.UserId, refreshToken).ConfigureAwait(false);

            var refreshTokenTask = _tokenManager.GenerateRefreshTokenAsync(userIdentity.UserId).ConfigureAwait(false);

            string newAccessToken = _tokenManager.GenerateAccessToken(userIdentity);
            string newRefreshToken = await refreshTokenTask;

            Response.SetTokenCookie(newAccessToken, userIdentity.IsPersistent, _tokensOptions.AccessToken);
            Response.SetTokenCookie(newRefreshToken, userIdentity.IsPersistent, _tokensOptions.RefreshToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Refreshing tokens failed at {UtcNow}", DateTime.UtcNow);
            return Problem();
        }

        return Ok();
    }

    [AllowAnonymous]
    [HttpGet("revoke")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
    public async Task<IActionResult> RevokeAsync()
    {
        string? accessToken = Request.GetTokenFromCookie(_tokensOptions.AccessToken);

        Response.ClearTokenCookie(_tokensOptions.AccessToken);
        Response.ClearTokenCookie(_tokensOptions.RefreshToken);

        if (string.IsNullOrWhiteSpace(accessToken) || !_tokenManager.TryGetUserIdentityFromAccessToken(accessToken, out UserIdentity userIdentity))
            return Unauthorized($"You are not authorized to perform this operation");

        try
        {
            await _tokenManager.RevokeActiveRefreshTokensAsync(userIdentity.UserId).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Revoking tokens failed at {UtcNow}", DateTime.UtcNow);
            return Problem();
        }

        return Ok();
    }

    [Authorize(Roles = "administrator")]
    [HttpGet("revoke/{userId:guid}")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
    public async Task<IActionResult> RevokeUserAsync([Required] Guid userId)
    {
        try
        {
            await _tokenManager.RevokeActiveRefreshTokensAsync(userId).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Revoking tokens of the user {UserId} failed at {UtcNow}", userId, DateTime.UtcNow);
            return Problem();
        }

        return Ok();
    }

    [AllowAnonymous]
    [HttpGet("jwks")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    public ActionResult<JsonWebKeySet> GetJwks()
    {
        var jwk = _tokenManager.GenerateAccessTokenJwk();

        return Ok(new[] { jwk });
    }

    [AllowAnonymous]
    [HttpGet("discovery")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    public IActionResult GetDiscoveryDocument()
    {
        var config = new OpenIdConnectConfiguration()
        {
            AuthorizationEndpoint = $"{_authOptions.Issuer}/api/v1/auth/authorize",
            JwksUri = $"{_authOptions.Issuer}/api/v1/auth/jwks",
            Issuer = _authOptions.Issuer
        };

        return Ok(config);
    }
}
