using MessageBroker.ActiveMQ.ManualInstrumentation;
using MessageBroker.Contract;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OrderService.Api.Data;
using OrderService.Api.Services;
using System.Reflection;

namespace DistributedTracingDotNet.Services.Orders.Api;

internal class Program
{
    private static readonly AssemblyName AssemblyName = typeof(Program).Assembly.GetName();
    private static readonly string ServiceName = AssemblyName.Name!;
    private static readonly string ServiceVersion = AssemblyName?.Version?.ToString() ?? "1.0.0";

    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Host.ConfigureLogging(logging => logging.ClearProviders());

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

        builder.Services.AddScoped<IOrderService, OrderService.Api.Services.OrderService>();
        builder.Services.AddScoped<IMessageSender, MessageSender>((_) => new MessageSender(new Uri("activemq:tcp://localhost:61616"), "OrderProcessing"));

        builder.Services.AddHttpClient();
        builder.Services.AddControllers();
        builder.Services.AddDbContext<DataContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

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