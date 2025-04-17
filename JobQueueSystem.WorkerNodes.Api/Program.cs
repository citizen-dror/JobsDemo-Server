using JobQueueSystem.QueueService.Services;
using JobsServer.Application.Interfaces;
using JobsServer.Domain.Interfaces.Repositories;
using JobsServer.Infrastructure.Repositories;
using JobsServer.Infrastructure;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
// Configure services
ConfigureServices(builder);

var app = builder.Build();

app.Logger.LogInformation("Starting workersNodes.Api in {Environment} environment", app.Environment.EnvironmentName);
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
// Enable CORS for specific origins
app.UseCors("AllowSpecificOrigins");

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

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

    // Register Worker services
    builder.Services.AddScoped<IWorkerRepository, WorkerRepository>();
    builder.Services.AddScoped<IWorkerService, WorkerService>();

    // Add SignalR services
    //builder.Services.AddSignalR();
}
