using Serilog;
using Serilog.Events;
using RideTheBus.Application.Games;
using RideTheBus.Infrastructure.Games;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddSerilog(configuration =>
    configuration.
        MinimumLevel.Information().
        MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning).
        WriteTo.Console());

builder.Services.AddScoped<CreateGame>();

builder.Services.AddSingleton<IGameSessionStore, InMemoryGameSessionStore>();


builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter());
    });

var app = builder.Build();

app.UseSerilogRequestLogging();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();