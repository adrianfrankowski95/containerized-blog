using Blog.Services.Identity.API.Core;
using Blog.Services.Identity.API.Infrastructure;
using Blog.Services.Identity.API.Models;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NodaTime;

var builder = WebApplication.CreateBuilder(args);

var isDevelopment = builder.Environment.IsDevelopment();

var config = GetConfiguration(isDevelopment);
// Add services to the container.

builder.Services
    .AddIdentityInfrastructure<User, Role>(config)
    .AddControllers();

builder.Services.AddRazorPages();

builder.Services
    .AddAuthentication(opts =>
        {
            opts.DefaultAuthenticateScheme = IdentityConstants.AuthenticationScheme;
            opts.RequireAuthenticatedSignIn = true;
        })
    .AddCookie(IdentityConstants.AuthenticationScheme, opts =>
    {
        opts.Cookie.HttpOnly = true;
        opts.Cookie.IsEssential = true;
        opts.Cookie.SameSite = SameSiteMode.Strict;
        opts.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    });

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (isDevelopment)
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapRazorPages();

app.Run();


static IConfiguration GetConfiguration(bool isDevelopment)
{
    string configFileName = isDevelopment ? "appsettings.Development.json" : "appsettings.json";

    return new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(configFileName, optional: false, reloadOnChange: true)
                .Build();
}

static class ServiceCollectionExtensions
{
    public static IServiceCollection AddNodaTimeClock(this IServiceCollection services)
    {
        services.TryAddSingleton<IClock, SystemClock>();

        return services;
    }
}