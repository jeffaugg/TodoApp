using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using TodoApp.Api.Telemetry;

namespace TodoApp.Telemetry.Extensions;

public static class TelemetryConfig
{
  public static void addTelemetry(this IServiceCollection services, IConfiguration configuration, ILoggingBuilder logging)
  {
    string otplEndpoint = configuration.GetConnectionString("OpenTelemetry:OtplEndpoint")!;

    services.AddOpenTelemetry().ConfigureResource(resource =>
        resource.AddService(serviceName: TelemetrySetup.ServiceName,
        serviceVersion: TelemetrySetup.ServiceVersion)
        // .AddAttributes(new Dictionary<string, object>
        // {
        //   ["deployment.environment"] = Environment.EnvironmentName
        // })
        ).WithMetrics(metrics =>
        {
          metrics.AddMeter(TelemetrySetup.Meter.Name);
          metrics.AddAspNetCoreInstrumentation().AddHttpClientInstrumentation().AddRuntimeInstrumentation();
          metrics.AddOtlpExporter(option => option.Endpoint = new Uri(otplEndpoint));
        }).WithTracing(trancing =>
        {
          trancing.AddSource(TelemetrySetup.activitySource.Name).SetErrorStatusOnException().AddAspNetCoreInstrumentation().AddHttpClientInstrumentation().AddEntityFrameworkCoreInstrumentation();
          trancing.AddOtlpExporter(option => option.Endpoint = new Uri(otplEndpoint));
        });

    logging.AddOpenTelemetry(logging =>
    {
      logging.AddOtlpExporter(option => option.Endpoint = new Uri(otplEndpoint));
    });
  }
}