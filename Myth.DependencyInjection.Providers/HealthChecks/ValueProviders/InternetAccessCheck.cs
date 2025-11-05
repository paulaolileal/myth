using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Myth.HealthChecks.ValueProviders {
    public class InternetAccessCheck : IHealthCheck {
        /// <summary>
        /// Checks if the internet connection is OK to be added to <c>Health Check</c> items
        /// </summary>
        /// <param name="context">Health check context</param>
        /// <param name="cancellationToken">The propagation cancellation token</param>
        /// <remarks>
        /// Example of usage:
        /// ```csharp
        /// services.AddHealthChecks()
        ///      .AddCheck<InternetAccessCheck>(
        ///          "Internet access",
        ///          tags: [ "internet" ]);
        /// ```
        /// </remarks>
        /// <returns>The current health check builder</returns>
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default) {
            var response = await Rest.Rest
                .Create()
                .Configure(x => x.WithTimeout(TimeSpan.FromSeconds(30)))
                .DoGet("https://www.google.com/")
                .OnResult(res => res.DoNotMap())
                .OnError(err => err.NotThrowForNonMappedResult())
                .BuildAsync(cancellationToken);

            var elapsedTime = response.ElapsedTime;
            var successStatusCode = response.IsSuccessStatusCode();

            if (successStatusCode && elapsedTime < TimeSpan.FromSeconds(10))
                return HealthCheckResult.Healthy();

            if (successStatusCode && elapsedTime > TimeSpan.FromSeconds(10))
                return HealthCheckResult.Degraded("The internet access is very slow or not working");

            return HealthCheckResult.Unhealthy("The internet access is not working");
        }
    }
}
