using Blog.Services.Blogging.API.Middlewares;

namespace Blog.Services.Blogging.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder builder)
     => builder.UseMiddleware<GlobalExceptionHandlerMiddleware>();
}