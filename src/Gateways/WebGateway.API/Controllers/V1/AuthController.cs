using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.RegularExpressions;
using Blog.Gateways.WebGateway.API.Config;
using Blog.Gateways.WebGateway.API.Extensions;
using Blog.Gateways.WebGateway.API.Models;
using Blog.Gateways.WebGateway.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Blog.Gateways.WebGateway.API.Controllers;

[Authorize]
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ILogger<AuthController> _logger;
    private readonly ITokenManager _tokenManager;
    private readonly JwtConfig _jwtOptions;

    public AuthController(ITokenManager tokenManager, IOptions<JwtConfig> jwtOptions, ILogger<AuthController> logger, ISysTime sysTime)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _tokenManager = tokenManager ?? throw new ArgumentNullException(nameof(tokenManager));
        _jwtOptions = jwtOptions.Value ?? throw new ArgumentNullException(nameof(jwtOptions));
    }

    [AllowAnonymous]
    [HttpGet("authorize")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
    public async Task<IActionResult> AuthorizeAsync()
    {
        UserCredentials userCredentials = new("232@#23.com", "password", true);

        if (string.IsNullOrWhiteSpace(userCredentials.Email)
            || string.IsNullOrWhiteSpace(userCredentials.Password)
            || !Regex.IsMatch(userCredentials.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250)))
            return Unauthorized($"Invalid {nameof(userCredentials.Email)} or {nameof(userCredentials.Password)}");


        //TODO: check with users microservice if credentials are valid
        if (false)
            return Unauthorized($"Invalid {nameof(userCredentials.Email)} or {nameof(userCredentials.Password)}");

        UserIdentity userIdentity = new(Guid.NewGuid(), "xyz@xyz.pl", "admin", "Administrator", userCredentials.RememberMe);

        try
        {
            var refreshTokenTask = _tokenManager.GenerateRefreshTokenAsync(userIdentity.UserId).ConfigureAwait(false);

            string newAccessToken = _tokenManager.GenerateAccessToken(userIdentity);
            string newRefreshToken = await refreshTokenTask;

            Response.SetTokenCookie(newAccessToken, userCredentials.RememberMe, _jwtOptions.AccessToken);
            Response.SetTokenCookie(newRefreshToken, userCredentials.RememberMe, _jwtOptions.RefreshToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Authorizing failed at {UtcNow}", DateTime.UtcNow);
            return Problem();
        }

        return Ok(userIdentity);
    }

    [AllowAnonymous]
    [HttpGet("refresh")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
    public async Task<IActionResult> RefreshAsync()
    {
        string? accessToken = Request.GetTokenFromCookie(_jwtOptions.AccessToken);
        string? refreshToken = Request.GetTokenFromCookie(_jwtOptions.RefreshToken);

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

            Response.SetTokenCookie(newAccessToken, userIdentity.IsPersistent, _jwtOptions.AccessToken);
            Response.SetTokenCookie(newRefreshToken, userIdentity.IsPersistent, _jwtOptions.RefreshToken);
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
        string? accessToken = Request.GetTokenFromCookie(_jwtOptions.AccessToken);

        Response.ClearTokenCookie(_jwtOptions.AccessToken);
        Response.ClearTokenCookie(_jwtOptions.RefreshToken);

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

    [Authorize(Roles = "Administrator,Moderator")]
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

    [HttpGet("key")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    public IActionResult GetKey()
    {
        return Ok(_tokenManager.GenerateAccessTokenJwk());
    }
}
