using System.Data;
using System.Net;
using System.Text.Json;
using Blog.Services.Blogging.API.Application.Exceptions;
using Blog.Services.Blogging.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Blog.Services.Blogging.API.Middlewares;

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
            var statusCode = ResolveHttpStatusCode(ex);

            if (statusCode is null)
                throw;

            var response = context.Response;

            response.ContentType = "application/json";
            response.StatusCode = (int)statusCode;

            await JsonSerializer.SerializeAsync(response.Body, ex.Message);

            _logger.LogInformation(ex, "----- Error handled globally with a HTTP response code {StatusCode}", Enum.GetName(statusCode.Value));
        }
    }

    private static HttpStatusCode? ResolveHttpStatusCode(Exception ex) => ex switch
    {
        BloggingDomainException
            => HttpStatusCode.BadRequest,

        KeyNotFoundException
            => HttpStatusCode.NotFound,

        DbUpdateConcurrencyException or DBConcurrencyException
            => HttpStatusCode.Conflict,

        DbUpdateException
            => HttpStatusCode.InternalServerError,

        IdentityException
            => HttpStatusCode.Unauthorized,

        _ => null
    };
}