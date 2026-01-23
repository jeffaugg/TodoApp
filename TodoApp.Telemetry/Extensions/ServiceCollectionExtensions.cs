using System.Linq;
using System.Reflection;
using Castle.DynamicProxy;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TodoApp.Telemetry.Attributes;
using TodoApp.Telemetry.Interceptors;

namespace TodoApp.Telemetry.Extensions;

public static class ServiceCollectionExtensions
{
    private static readonly ProxyGenerator _proxyGenerator = new();
    private static readonly TelemetryInterceptor _interceptor = new();

    public static IServiceCollection AddTelemetryInterception(this IServiceCollection services)
    {
        // Intercepta serviços que têm o atributo [Telemetry]
        var serviceDescriptors = services
            .Where(s => s.ServiceType.IsInterface && s.ImplementationType != null)
            .ToList();

        foreach (var descriptor in serviceDescriptors)
        {
            if (descriptor.ImplementationType != null)
            {
                var hasTelemetryAttribute = descriptor.ImplementationType
                    .GetCustomAttributes(typeof(TelemetryAttribute), false)
                    .Any();

                if (hasTelemetryAttribute)
                {
                    // Remove o registro original usando RemoveAll para garantir remoção
                    services.RemoveAll(descriptor.ServiceType);

                    // Adiciona com proxy
                    services.Add(new ServiceDescriptor(
                        descriptor.ServiceType,
                        sp =>
                        {
                            var target = ActivatorUtilities.CreateInstance(sp, descriptor.ImplementationType!);
                            return _proxyGenerator.CreateInterfaceProxyWithTarget(
                                descriptor.ServiceType,
                                target,
                                _interceptor);
                        },
                        descriptor.Lifetime));
                }
            }
        }

        return services;
    }
}
