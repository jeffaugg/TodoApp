using System;
using System.Diagnostics;
using Castle.DynamicProxy;
using TodoApp.Api.Telemetry;
using TodoApp.Telemetry.Utils;

namespace TodoApp.Telemetry.Interceptors;

public class TelemetryInterceptor : IInterceptor
{
    public void Intercept(IInvocation invocation)
    {
        var className = invocation.TargetType.Name;
        var methodName = invocation.Method.Name;
        var activityName = $"{className}.{methodName}";

        using var activity = TelemetrySetup.activitySource.StartActivity(activityName);

        if (activity != null)
        {
            activity.SetTag("step", NamingHelper.ToSnakeCaseStep(className, methodName));
        }

        try
        {
            invocation.Proceed();
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.SetTag("error", true);
            activity?.SetTag("error.message", ex.Message);
            throw;
        }
    }
}
