using System.Security.Cryptography;
using Blog.Gateways.WebGateway.API.Config;
using Blog.Gateways.WebGateway.API.Controllers;
using Blog.Gateways.WebGateway.API.Extensions;
using Blog.Gateways.WebGateway.API.Infrastructure;
using Blog.Gateways.WebGateway.API.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

bool isDevelopment = builder.Environment.IsDevelopment();
var config = GetConfiguration(isDevelopment);

var services = builder.Services;

// Add services to the container.
services.AddSwaggerGen();

services.AddOptions<UrlsConfig>().Bind(config.GetRequiredSection(UrlsConfig.Section));
services
    .AddGatewayControllers(isDevelopment)
    .AddCustomJwtAuthentication(config);

//await AuthContextSeed.SeedAsync(config);

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

app.Run();

static IConfiguration GetConfiguration(bool isDevelopment)
{
    string configFileName = isDevelopment ? "appsettings.Development.json" : "appsettings.json";

    return new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(configFileName, optional: false, reloadOnChange: true)
                .Build();
}

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCustomJwtAuthentication(this IServiceCollection services, IConfiguration config)
    {
        services.AddOptions<JwtConfig>().Bind(config.GetRequiredSection(JwtConfig.Section));
        services
            .AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<JwtConfig>, IOptions<UrlsConfig>>((opts, jwtOptions, urlsOptions) =>
            {
                var accessTokenOptions = jwtOptions.Value.AccessToken;

                opts.MapInboundClaims = false;
                opts.RequireHttpsMetadata = true;
                opts.Authority = accessTokenOptions.Authority;
                opts.ClaimsIssuer = accessTokenOptions.Issuer;
                opts.MetadataAddress = UrlsConfig.AuthActions.GetDiscovery();

                opts.TokenValidationParameters = new TokenValidationParameters
                {
                    RequireSignedTokens = true,
                    RequireExpirationTime = true,
                    ValidateLifetime = true,
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = accessTokenOptions.Issuer,
                    ValidAudience = accessTokenOptions.Audience,
                    ValidAlgorithms = new[] { SecurityAlgorithms.RsaSha256 },
                    ClockSkew = TimeSpan.Zero,
                    NameClaimType = UserClaimTypes.Name,
                    RoleClaimType = UserClaimTypes.Role,
                    AuthenticationType = JwtBearerDefaults.AuthenticationScheme
                };
                opts.Events = new JwtBearerEvents()
                {
                    OnMessageReceived = context =>
                    {
                        context.Token = context.HttpContext.Request.GetTokenFromCookie(accessTokenOptions);

                        return Task.CompletedTask;
                    }
                };

            });

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, opts => { });

        return services;
    }
}