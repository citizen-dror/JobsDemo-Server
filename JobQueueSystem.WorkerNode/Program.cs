using JobsServer.Domain.Interfaces.Services;
using JobsServer.Domain.Interfaces.APIs;
using JobsServer.Domain.Entities;
using JobQueueSystem.WorkerNodes.Services;
using JobsServer.Infrastructure.RabbitMQ;
using JobQueueSystem.Core.Configs;

var builder = Host.CreateApplicationBuilder(args);
ConfigureServices(builder.Services);

var host = builder.Build();
host.Run();

void ConfigureServices(IServiceCollection services)
{
    // Configuration
    services.Configure<WorkerSettings>(builder.Configuration.GetSection("WorkerSettings"));
    //services.Configure<RabbitMQConfig>(builder.Configuration.GetSection("RabbitMQ"));

    builder.Services.AddHttpClient(); // Registers IHttpClientFactory

    // Register RabbitMQ services
    var rabbitMqConfig = builder.Configuration.GetSection("RabbitMQ").Get<RabbitMQConfig>();
    services.AddSingleton(rabbitMqConfig);
    services.AddSingleton<RabbitConnectionFactory>();

    // Register RabbitReceiver as Transient with manual parameter binding
    services.AddTransient<RabbitReceiver>(provider =>
    {
        var connectionFactory = provider.GetRequiredService<RabbitConnectionFactory>();
        var logger = provider.GetRequiredService<ILogger<RabbitReceiver>>();

        return new RabbitReceiver(
            connectionFactory,
            logger,
            exchangeName: "worker.jobs",
            routingKey: "default",
            durable: true,
            exclusive: false,
            autoDelete: false);
    });

    // Other services
    services.AddScoped<IWorkerApiClient, WorkerApiClient>();
    services.AddSingleton<IJobProcessor, JobProcessor>();
    services.AddHostedService<WorkerNodeService>();
}
