using Blog.Services.Blogging.API.Middlewares;

namespace Blog.Services.Blogging.API.Extensions;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder builder)
     => builder.UseMiddleware<GlobalExceptionHandlerMiddleware>();
}