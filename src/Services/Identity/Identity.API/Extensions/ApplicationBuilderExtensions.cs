using Blog.Services.Identity.API.Middlewares;

namespace Blog.Services.Identity.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder builder)
     => builder.UseMiddleware<GlobalExceptionHandlerMiddleware>();
}