using Microsoft.Extensions.DependencyInjection.Extensions;
using NodaTime;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();


static class ServiceCollectionExtensions
{
    public static IServiceCollection AddNodaTimeClock(this IServiceCollection services)
    {
        services.TryAddSingleton<IClock, SystemClock>();

        return services;
    }
}