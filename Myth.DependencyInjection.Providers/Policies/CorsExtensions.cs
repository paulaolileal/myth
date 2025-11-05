using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Myth.Policies {

    public static class ServicesExtensions {

        /// <summary>
        /// Add a default CORS to the application service collection
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        /// <remarks>
        /// This method add a policy allowing:
        /// <list type="bullet">
        ///     <item>
        ///         <term>Any Origin</term>
        ///         <description>All urls/IPs are valid for call the application</description>
        ///     </item>
        ///     <item>
        ///         <term>Any Method</term>
        ///         <description>All REST methods can be called</description>
        ///     </item>
        ///     <item>
        ///         <term>Any header</term>
        ///         <description>All header are valid</description>
        ///     </item>
        /// </list>
        /// </remarks>
        public static IServiceCollection AddDefaultCors(this IServiceCollection services) {
            services.AddCors(options => {
                options.AddPolicy("*", builder => builder
                    .AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader());
            });

            return services;
        }

        /// <summary>
        /// Uses the default CORS policy previously created
        /// </summary>
        /// <param name="app">The application builder pipeline</param>
        /// <returns>The application builder pipeline</returns>
        public static IApplicationBuilder UseDefaultCors(this IApplicationBuilder app) {
            app.UseCors("*");

            return app;
        }
    }
}