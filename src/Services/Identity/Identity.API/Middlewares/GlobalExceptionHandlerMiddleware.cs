using System.Data;
using System.Net;
using System.Text.Json;
using Blog.Services.Identity.Domain.Exceptions;
using Blog.Services.Identity.Infrastructure.Idempotency;
using Microsoft.EntityFrameworkCore;

namespace Blog.Services.Identity.API.Middlewares;

public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

    public GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            var statusCode = GetHttpCode(ex);
            var response = context.Response;
            response.ContentType = "application/json";
            response.StatusCode = (int)statusCode;
            await JsonSerializer.SerializeAsync(response.Body, ex.Message);

            _logger.LogError(ex, "----- Error handled globally with an HTTP response code {StatusCode}", Enum.GetName(statusCode));
        }
    }

    private static HttpStatusCode GetHttpCode(Exception ex) => ex switch
    {
        IdentityDomainException
            => HttpStatusCode.BadRequest,

        KeyNotFoundException
            => HttpStatusCode.NotFound,

        IdempotencyException
            => HttpStatusCode.BadRequest,

        DbUpdateConcurrencyException or DBConcurrencyException
            => HttpStatusCode.Conflict,

        DbUpdateException
            => HttpStatusCode.InternalServerError,

        _ => HttpStatusCode.InternalServerError
    };
}