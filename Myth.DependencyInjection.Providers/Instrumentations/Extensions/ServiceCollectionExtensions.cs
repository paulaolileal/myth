using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Myth.Extensions;
using Myth.Instrumentations.Interfaces;
using Myth.Instrumentations.Settings;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Myth.Instrumentations.Extensions {
    public static class ServiceCollectionExtensions {

        /// <summary>
        /// Add the collectible tracing metrics types to service collection
        /// </summary>
        /// <param name="services">The application service collection</param>
        /// <returns>The application service collection</returns>
        public static IServiceCollection AddCollectibleMetrics(this IServiceCollection services) {
            services.AddServiceFromType<ICustomMetric>();

            return services;
        }

        /// <summary>
        /// Adds and configures OpenTelemetry metrics and tracing to the service collection.
        /// Allows customization of metrics and tracing settings via optional delegates.
        /// Registers default instrumentations and exporters based on the environment.
        /// </summary>
        public static IServiceCollection AddTelemetry(this IServiceCollection services, Action<TelemetrySettings> settings) {
            var serviceProvider = services.BuildServiceProvider();
            var environment = serviceProvider.GetRequiredService<IHostEnvironment>();

            var telemetrySettings = new TelemetrySettings();

            services.AddMetrics();
            services
                 .AddOpenTelemetry()
                 .ConfigureResource(resource => resource
                     .AddService(telemetrySettings.ApplicationName))
                     .WithMetrics(metrics => {
                         metrics
                             .AddMeter("Microsoft.AspNetCore.Hosting")
                             .AddMeter("Microsoft.AspNetCore.Server.Kestrel")
                             .AddAspNetCoreInstrumentation()
                             .AddHttpClientInstrumentation()
                             .AddRuntimeInstrumentation()
                             .AddProcessInstrumentation()
                             .AddPrometheusExporter();

                         telemetrySettings.MetricsProviderBuilder?.Invoke(metrics);
                     })
                     .WithTracing(tracing => {
                         tracing
                            .AddAspNetCoreInstrumentation()
                            .AddHttpClientInstrumentation()
                            .AddSqlClientInstrumentation();

                         if (environment.IsProduction() || environment.IsStaging()) {
                             tracing.AddOtlpExporter(options => options
                                .Endpoint = new Uri(telemetrySettings.OpenTelemetryUrl));
                         }
                         else {
                             tracing.AddConsoleExporter();
                         }

                         telemetrySettings.TracingProviderBuilder?.Invoke(tracing);
                     });

            return services;
        }

        public static IMvcBuilder AddControllerWithMetrics(this IServiceCollection services) {
            return services
                .AddControllers()
                .AddMetricsController();
        }
    }

}
