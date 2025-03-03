using Microsoft.EntityFrameworkCore;
using JobsServer.Application.Services;
using JobsServer.Application.Mappings;
using JobsServer.Infrastructure.Repositories;
using JobsServer.Infrastructure;
using JobsServer.Api.Middleware;
using JobsServer.Api.SignalR;
using JobsServer.Application.Notifications;

var builder = WebApplication.CreateBuilder(args);
// Configure services
ConfigureServices(builder);
// Build the app
var app = builder.Build();
// Configure middleware and request pipeline
ConfigureMiddleware(app);
// Map controllers and SignalR hubs
MapEndpoints(app);
try
{
    app.Run();
}
catch (Exception ex)
{
    app.Logger.LogCritical(ex, "Application startup failed");
    throw;
}

void ConfigureServices(WebApplicationBuilder builder)
{
    // Logging
    builder.Logging.AddConsole();

    // Core MVC and Swagger
    builder.Services.AddControllers()
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        });

    // Swagger/OpenAPI
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    // Add DbContext with connection string
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

    // Read CORS settings from config
    var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? new string[0];
    var allowedMethods = builder.Configuration.GetSection("AllowedMethods").Get<string[]>() ?? new string[] { "GET", "POST", "PUT", "DELETE", "OPTIONS" };
    var allowedHeaders = builder.Configuration.GetSection("AllowedHeaders").Get<string[]>() ?? new string[] { "Content-Type", "Authorization", "Accept", "X-Requested-With" };
    // Add CORS policy
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowSpecificOrigins", policy =>
        {
            policy.WithOrigins(allowedOrigins)
                .AllowCredentials()  // Allow credentials (cookies, headers) for signal-R
                .WithMethods(allowedMethods)
                .WithHeaders(allowedHeaders)
                .WithExposedHeaders("x-requested-with")
                .AllowAnyHeader();
        });
    });

    // Infrastructure
    builder.Services.AddInfrastructure(builder.Configuration);

    // Register AutoMapper
    builder.Services.AddAutoMapper(typeof(JobProfile));
    // Register Notifier
    builder.Services.AddSingleton<IJobUpdateNotifier, JobUpdateNotifier>();

    // Register Job services
    builder.Services.AddScoped<IJobRepository, JobRepository>();
    builder.Services.AddScoped<IJobService, JobService>();

    // Add SignalR services
    builder.Services.AddSignalR();
}

void ConfigureMiddleware(WebApplication app)
{
    app.Logger.LogInformation("Starting application in {Environment} environment", app.Environment.EnvironmentName);
    // Configure the HTTP request pipeline
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
}

void MapEndpoints(WebApplication app)
{
    app.MapControllers();
    // Map SignalR hubs
    app.MapHub<JobHub>("/jobHub");
}
