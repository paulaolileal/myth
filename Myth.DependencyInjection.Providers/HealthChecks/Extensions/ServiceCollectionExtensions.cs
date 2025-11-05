using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Myth.HealthChecks.Settings;
using Myth.HealthChecks.ValueProviders;
using Myth.ServiceProvider;
using Prometheus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Myth.HealthChecks.Extensions {
    public static class ServiceCollectionExtensions {
        /// <summary>
        /// Add a <c>Health Check</c> to the application service collection
        /// </summary>
        /// <param name="services">The application service collection</param>
        /// <param name="configuration">The configurations with app settings</param>
        /// <returns>The application service collection</returns>
        public static IServiceCollection AddHealthCheck(this IServiceCollection services, Action<HealCheckSettings>? settings = default) {
            var serviceProvider = MythServiceProvider.GetOrFallback(services.BuildServiceProvider());

            var configuration = serviceProvider?.GetRequiredService<IConfiguration>();

            var healthCheckBuilder = services.AddHealthChecks();

            var healCheckSettings = new HealCheckSettings(healthCheckBuilder);
            settings?.Invoke(healCheckSettings);

            if (healCheckSettings.CheckInternetAccess)
                healthCheckBuilder
                    .AddCheck<InternetAccessCheck>(
                        "Internet access",
                        tags: ["internet", "environment"]);

            foreach (var databaseConnection in healCheckSettings.DatabaseConnections) {
                var connectionString = configuration.GetConnectionString(databaseConnection.Key);

                if (string.IsNullOrEmpty(connectionString))
                    continue;

                healthCheckBuilder = databaseConnection.Value switch {
                    DatabaseProvider.SQLServer =>
                        healthCheckBuilder.AddSqlServer(
                            connectionString,
                            name: "SQL Server Database",
                            tags: ["database", "sqlserver", "sql"]),

                    DatabaseProvider.PostgreSQL =>
                        healthCheckBuilder.AddNpgSql(
                            connectionString,
                            name: "PostgreSQL Database",
                            tags: ["database", "postgresql", "sql"]),

                    DatabaseProvider.MongoDB =>
                        healthCheckBuilder.AddMongoDb(
                        connectionString,
                        name: "MongoDB Database",
                        tags: ["database", "mongo", "no-sql"]),

                    DatabaseProvider.Redis =>
                        healthCheckBuilder.AddRedis(
                        connectionString,
                        name: "Redis",
                        tags: ["cache", "redis"]),

                    _ => throw new NotSupportedException("Provider not supported")
                };
            }

            return services;
        }

        /// <summary>
        /// Uses the http metrics in the application
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseMetrics(this IApplicationBuilder app) {
            app.UseHttpMetrics();

            return app;
        }
    }
}
