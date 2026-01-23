using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace TodoApp.Api.Telemetry
{
  public class TelemetrySetup
  {
    public const string ServiceName = "TodoApp";

    public const string ServiceVersion = "1.0.0";

    public static readonly ActivitySource activitySource = new(ServiceName);

    public static Meter Meter = new(ServiceName);

    public static Counter<int> UsersCreatedCounter = Meter.CreateCounter<int>("users_created", description: "Counts the number of users created");
  }
}