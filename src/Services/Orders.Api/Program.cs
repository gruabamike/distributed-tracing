using MessageBroker.ActiveMQ.ManualInstrumentation;
using MessageBroker.Contract;
using Microsoft.EntityFrameworkCore;
using Microsoft.Net.Http.Headers;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Orders.Api.Data;
using Orders.Api.Services;
using System.Net.Mime;
using System.Reflection;

namespace Orders.Api;

internal class Program
{
    private const string BROKER_URI_ENVIRONMENT_VARIABLE_NAME = "BROKER_URI";
    private const string USERS_API_URI_ENVIRONMENT_VARIALBE_NAME = "USERS_API_URI";
    private const string INVENTORY_API_URI_ENVIRONMENT_VARIALBE_NAME = "INVENTORY_API_URI";
    private const string ORDER_QUEUE_NAME_ENVIRONMENT_VARIABLE_NAME = "ORDER_PROCESSING_QUEUE_NAME";

    private static readonly AssemblyName AssemblyName = typeof(Program).Assembly.GetName();
    private static readonly string ServiceName = AssemblyName.Name!;
    private static readonly string ServiceVersion = AssemblyName?.Version?.ToString() ?? "1.0.0";

    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

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
                .AddSource(ActiveMQSourceInfoProvider.ActivitySourceName)
                .SetResourceBuilder(GetResourceBuilder())
                .AddHttpClientInstrumentation()
                .AddAspNetCoreInstrumentation()
                .AddSqlClientInstrumentation()
            )
            .StartWithHost();

        builder.Services.AddScoped<IOrderService, OrderService>();
        builder.Services.AddScoped<IMessageSender, MessageSender>((_)
            => new MessageSender(
                new ActiveMQContextPropagationHandler(),
                new Uri(Environment.GetEnvironmentVariable(BROKER_URI_ENVIRONMENT_VARIABLE_NAME)!),
                Environment.GetEnvironmentVariable(ORDER_QUEUE_NAME_ENVIRONMENT_VARIABLE_NAME)!));

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

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }

    private static ResourceBuilder GetResourceBuilder()
        => ResourceBuilder.CreateDefault().AddService(ServiceName, ServiceVersion);
}