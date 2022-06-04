using Blog.Services.Identity.API.Core;
using Blog.Services.Identity.API.Infrastructure;
using Blog.Services.Identity.API.Models;
using Blog.Services.Identity.API.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NodaTime;


var builder = WebApplication.CreateBuilder(args);

var isDevelopment = builder.Environment.IsDevelopment();

var config = GetConfiguration(isDevelopment);

// Add services to the container.

builder.Services.AddRazorPages().AddCookieTempDataProvider(opts =>
{
    opts.Cookie.HttpOnly = true;
    opts.Cookie.IsEssential = true;
    opts.Cookie.SameSite = SameSiteMode.Strict;
    opts.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});

builder.Services
    .AddCustomIdentityInfrastructure<User, Role>(config)
    .AddCustomIdentityCore<User>()
    .AddCustomServices()
    .AddControllers();

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
app.UseStaticFiles(); //html, css, images, js in wwwroot folder

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