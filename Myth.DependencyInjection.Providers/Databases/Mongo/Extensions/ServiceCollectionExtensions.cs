using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Myth.Databases.Mongo.Settings;

namespace Myth.Databases.Mongo.Extensions {

    public static class ServiceCollectionExtensions {

        /// <summary>
        /// Registers MongoDB client and database services in the dependency injection container.
        /// <para>
        /// Requires the following keys in <c>appSettings.json</c>:
        /// <list type="bullet">
        ///   <item>
        ///     <description><c>ConnectionStrings:MongoDatabase</c> - The MongoDB connection string.</description>
        ///   </item>
        ///   <item>
        ///     <description><c>Mongo:DatabaseName</c> - The name of the MongoDB database.</description>
        ///   </item>
        /// </list>
        /// </para>
        /// <para>
        /// Note: Connection string tokens will be automatically replaced with values from the <b>vault</b> if they exist.
        /// Tokens follow the pattern:
        /// <list type="bullet">
        ///   <item><description><c>#{mongo-server}#</c> - The MongoDB server address</description></item>
        ///   <item><description><c>#{mongo-user}#</c> - The MongoDB username</description></item>
        ///   <item><description><c>#{mongo-password}#</c> - The MongoDB password</description></item>
        /// </list>
        /// </para>
        /// <para>
        /// The connection string in appSettings should follow this pattern: <c>"#{mongo-user}#:#{mongo-password}#@#{mongo-server}#"</c>
        /// </para>
        /// <para>
        /// Exceptions:
        /// <list type="bullet">
        ///   <item>
        ///     <description><see cref="InvalidOperationException"/> if the connection string or database name is not configured.</description>
        ///   </item>
        /// </list>
        /// </para>
        /// </summary>
        public static IServiceCollection AddMongoDatabase(this IServiceCollection services, Action<MongoSettings> settings) {
            var mongoSettings = new MongoSettings();
            settings?.Invoke(mongoSettings);

            services.AddSingleton<IMongoClient>(serviceProvider => {
                var configuration = serviceProvider.GetRequiredService<IConfiguration>();
                var connectionString = configuration.GetConnectionString(mongoSettings.ConnectionStringKey);

                if (string.IsNullOrEmpty(connectionString))
                    throw new InvalidOperationException("MongoDB connection string is not configured.");

                return new MongoClient(connectionString);
            });

            services.AddScoped<IMongoDatabase>(serviceProvider => {
                var configuration = serviceProvider.GetRequiredService<IConfiguration>();
                var connectionString = configuration.GetConnectionString(mongoSettings.ConnectionStringKey);
                var mongoUrl = new MongoUrl(connectionString);

                var client = serviceProvider.GetRequiredService<IMongoClient>();

                var databaseName = mongoSettings.DatabaseName ?? mongoUrl.DatabaseName;

                if (string.IsNullOrEmpty(databaseName))
                    throw new InvalidOperationException("MongoDB database name is not configured.");

                return client.GetDatabase(databaseName);
            });

            return services;
        }
    }
}