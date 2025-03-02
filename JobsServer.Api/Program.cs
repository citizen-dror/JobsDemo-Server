using JobsServer.Application.Services;
using JobsServer.Infrastructure.Repositories;
using JobsServer.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using JobsServer.Api.Middleware;
using Microsoft.AspNetCore.Hosting;
using JobsServer.Application.Mappings;

var builder = WebApplication.CreateBuilder(args);

// Logging
builder.Logging.AddConsole();
// Core MVC and Swagger
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve;
    });
// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add DbContext with connection string
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Read CORS settings from config
var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? new string[0];
var allowedMethods = builder.Configuration.GetSection("AllowedMethods").Get<string[]>() ?? new string[] { "GET", "POST" };
var allowedHeaders = builder.Configuration.GetSection("AllowedHeaders").Get<string[]>() ?? new string[] { "Content-Type" };
// Add CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .WithMethods(allowedMethods)
              .WithHeaders(allowedHeaders);
    });
});

// Infrastructure
builder.Services.AddInfrastructure(builder.Configuration);
// Register AutoMapper
builder.Services.AddAutoMapper(typeof(JobProfile));
// Register Job services
builder.Services.AddScoped<IJobRepository, JobRepository>();
builder.Services.AddScoped<IJobService, JobService>();


var app = builder.Build();
app.Logger.LogInformation("Starting application in {Environment} environment", app.Environment.EnvironmentName);
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
// Enable CORS for specific origins
app.UseCors("AllowSpecificOrigins");

app.UseHttpsRedirection();
// Register the ApiLogger middleware
app.UseMiddleware<ApiLogger>();
app.UseAuthorization();
app.MapControllers();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast")
.WithOpenApi();

try
{
    app.Run();
}
catch (Exception ex)
{
    app.Logger.LogCritical(ex, "Application startup failed");
    throw;
}

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
