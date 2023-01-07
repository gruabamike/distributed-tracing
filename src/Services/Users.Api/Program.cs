using DistributedTracingDotNet.Services.Users.Api.Data;
using DistributedTracingDotNet.Services.Users.Api.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Reflection;

internal class Program
{
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
            options.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(ServiceName, ServiceVersion));
            options.AddOtlpExporter(options => options.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.HttpProtobuf);
        });

        builder.Services.AddOpenTelemetry()
            .WithMetrics(builder => builder
                .AddOtlpExporter(options => options.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.HttpProtobuf)
                .SetResourceBuilder(GetResourceBuilder())
                .AddHttpClientInstrumentation()
                .AddAspNetCoreInstrumentation())
            .WithTracing(builder => builder
                .AddOtlpExporter(options => options.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.HttpProtobuf)
                .AddSource(ServiceName)
                .SetResourceBuilder(GetResourceBuilder())
                .AddHttpClientInstrumentation((options) =>
                {
                    options.EnrichWithHttpRequestMessage = (activity, httpRequestMessage) => activity.SetTag("requestVersion", httpRequestMessage.Version);
                    options.EnrichWithHttpResponseMessage = (activity, httpResponseMessage) => activity.SetTag("requestVersion", httpResponseMessage.Version);
                    options.EnrichWithException = (activity, exception) => activity.SetTag("stackTrace", exception.StackTrace);
                })
                .AddAspNetCoreInstrumentation(options =>
                {
                    options.EnrichWithHttpRequest = (activity, httpRequest) =>
                    {
                        activity.SetTag("requestProtocol", httpRequest.Protocol);
                    };
                    options.EnrichWithHttpResponse = (activity, httpResponse) =>
                    {
                        activity.SetTag("responseLength", httpResponse.ContentLength);
                    };
                    options.EnrichWithException = (activity, exception) =>
                    {
                        activity.SetTag("exceptionType", exception.GetType().ToString());
                    };
                })
                .AddSqlClientInstrumentation(options =>
                {
                    options.SetDbStatementForText = true;
                    options.SetDbStatementForStoredProcedure = true;
                    options.EnableConnectionLevelAttributes = true;
                }
                ))
            .StartWithHost();

        builder.Services.AddControllers();
        builder.Services.AddDbContext<DataContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

        builder.Services.AddScoped<IUserService, UserService>();

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
        => ResourceBuilder.CreateDefault().AddService(
            serviceName: ServiceName,
            serviceNamespace: ServiceName,
            serviceVersion: ServiceVersion);
}