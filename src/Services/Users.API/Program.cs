using DistributedTracingDotNet.Services.Users.API.Data;
using DistributedTracingDotNet.Services.Users.API.Services;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Reflection;

var serviceName = $"{nameof(DistributedTracingDotNet)}.Users.API";
var serviceVersion = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0.0";

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.


builder.Host.ConfigureLogging(logging => logging.ClearProviders());
builder.Logging.AddOpenTelemetry(options =>
{
    options.IncludeFormattedMessage = true;
    options.IncludeScopes = true;
    options.ParseStateValues = true;
    options.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName, serviceVersion: serviceVersion));
    options.AddOtlpExporter(options => options.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.HttpProtobuf);
});

builder.Services.AddOpenTelemetryMetrics(builder => builder
    .AddOtlpExporter(options => options.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.HttpProtobuf)
    .SetResourceBuilder(
        ResourceBuilder.CreateDefault()
            .AddService(serviceName, serviceVersion: serviceVersion))
    .AddHttpClientInstrumentation()
    .AddAspNetCoreInstrumentation()
);

builder.Services.AddOpenTelemetryTracing(builder => builder
    .AddOtlpExporter(options => options.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.HttpProtobuf)
    .AddSource(serviceName)
    .SetResourceBuilder(
        ResourceBuilder.CreateDefault()
            .AddService(serviceName, serviceVersion: serviceVersion))
    .AddHttpClientInstrumentation()
    .AddAspNetCoreInstrumentation()
    .AddSqlClientInstrumentation()
);

builder.Services.AddControllers();
builder.Services.AddDbContext<DataContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IUserService, UserService>();

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
