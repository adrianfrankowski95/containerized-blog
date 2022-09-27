using Blog.Services.Identity.API.Middlewares;

namespace Blog.Services.Identity.API.Extensions;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder builder)
     => builder.UseMiddleware<GlobalExceptionHandlerMiddleware>();
}