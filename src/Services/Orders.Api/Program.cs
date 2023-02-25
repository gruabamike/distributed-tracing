using MessageBroker.ActiveMQ.AutoInstrumentation;
using MessageBroker.Contract;
using Microsoft.EntityFrameworkCore;
using Microsoft.Net.Http.Headers;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Orders.Api.Data;
using Orders.Api.Provider;
using Orders.Api.Services;
using Polly;
using System.Net.Mime;
using System.Reflection;

namespace Orders.Api;

internal class Program
{
    private const int MAX_BROKER_CONNETION_RETRIES = 5;

    private const string BROKER_URI_ENVIRONMENT_VARIABLE_NAME = "BROKER_URI";
    private const string USERS_API_URI_ENVIRONMENT_VARIALBE_NAME = "USERS_API_URI";
    private const string INVENTORY_API_URI_ENVIRONMENT_VARIALBE_NAME = "INVENTORY_API_URI";
    private const string ORDER_QUEUE_NAME_ENVIRONMENT_VARIABLE_NAME = "ORDER_PROCESSING_QUEUE_NAME";

    private static readonly AssemblyName AssemblyName = typeof(Program).Assembly.GetName();
    private static readonly string ServiceName = AssemblyName.Name!;
    private static readonly string ServiceVersion = AssemblyName?.Version?.ToString() ?? "1.0.0";

    private static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        var logger = LoggerFactory.Create(config =>
        {
            config.AddConsole();
            config.AddConfiguration(builder.Configuration.GetSection("Logging"));
        }).CreateLogger(ServiceName);

        builder.Logging.AddOpenTelemetry(options =>
        {
            options.IncludeFormattedMessage = true;
            options.IncludeScopes = true;
            options.ParseStateValues = true;
            options.SetResourceBuilder(GetResourceBuilder());
            options.AddOtlpExporter(options => options.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.HttpProtobuf);
        });

        builder.Services.AddOpenTelemetry()
            .WithMetrics(builder => builder
                .AddOtlpExporter(options => options.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.HttpProtobuf)
                .SetResourceBuilder(GetResourceBuilder())
                .AddHttpClientInstrumentation()
                .AddAspNetCoreInstrumentation()
            )
            .WithTracing(builder => builder
                .AddOtlpExporter(options => options.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.HttpProtobuf)
                .SetResourceBuilder(GetResourceBuilder())
                .AddHttpClientInstrumentation()
                .AddAspNetCoreInstrumentation()
                .AddSqlClientInstrumentation()
                .AddActiveMQInstrumentation()
            )
            .StartWithHost();

        using ActiveMQConnection activeMQConnection = new ActiveMQConnection(
            new Uri(Environment.GetEnvironmentVariable(BROKER_URI_ENVIRONMENT_VARIABLE_NAME)!),
            ServiceName);

        builder.Services.AddScoped<IQueueNameProvider, QueueNameProvider>(_ =>
            new QueueNameProvider(Environment.GetEnvironmentVariable(ORDER_QUEUE_NAME_ENVIRONMENT_VARIABLE_NAME)!));
        builder.Services.AddScoped<IOrderService, OrderService>();
        builder.Services.AddScoped<IActiveMQConnection, ActiveMQConnection>(_ => activeMQConnection);
        builder.Services.AddScoped<IMessageSender, MessageSender>();

        builder.Services.AddHttpClient("Users", httpClient =>
        {
            httpClient.BaseAddress = new Uri(Environment.GetEnvironmentVariable(USERS_API_URI_ENVIRONMENT_VARIALBE_NAME)!);
            httpClient.DefaultRequestHeaders.Add(HeaderNames.Accept, MediaTypeNames.Application.Json);
            httpClient.DefaultRequestHeaders.Add(HeaderNames.UserAgent, nameof(Orders.Api));
        });
        builder.Services.AddHttpClient("Inventory", httpClient =>
        {
            httpClient.BaseAddress = new Uri(Environment.GetEnvironmentVariable(INVENTORY_API_URI_ENVIRONMENT_VARIALBE_NAME)!);
            httpClient.DefaultRequestHeaders.Add(HeaderNames.Accept, MediaTypeNames.Application.Json);
            httpClient.DefaultRequestHeaders.Add(HeaderNames.UserAgent, nameof(Orders.Api));
        });
        builder.Services.AddControllers();
        builder.Services.AddDbContext<DataContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

        builder.Services.AddAutoMapper(typeof(Program));
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        builder.Services.AddRouting(option => option.LowercaseUrls = true);
        var app = builder.Build();

        using (var scope = app.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            var context = services.GetRequiredService<DataContext>();
            ApplyMigrations(context);
        }

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.MapControllers();

        var retryPolicy = Policy
            .Handle<Apache.NMS.NMSConnectionException>()
            .WaitAndRetryAsync(
                MAX_BROKER_CONNETION_RETRIES,
                (retryAttempt) =>
                {
                    var sleepDuration = TimeSpan.FromSeconds(Math.Pow(2, retryAttempt) / 2);
                    logger.LogInformation($"retry connecting to message broker (#{retryAttempt}; sleepDuration: {sleepDuration.Seconds}s)");
                    return sleepDuration;
                });

        await retryPolicy.ExecuteAsync(() => activeMQConnection.OpenAsync());
        logger.LogInformation("successfully connected to message broker");

        app.Run();
    }

    private static ResourceBuilder GetResourceBuilder()
        => ResourceBuilder.CreateDefault().AddService(
            serviceName: ServiceName,
            serviceNamespace: ServiceName,
            serviceVersion: ServiceVersion);
    private static void ApplyMigrations(DataContext context)
    {
        if (context.Database.GetPendingMigrations().Any())
        {
            context.Database.Migrate();
        }
    }
}