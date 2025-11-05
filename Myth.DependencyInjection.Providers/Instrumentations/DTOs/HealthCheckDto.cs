namespace Myth.Instrumentations.DTOs {

    /// <summary>
    /// Represents the overall health status summary for the application including all health check results
    /// </summary>
    /// <remarks>
    /// This DTO aggregates the results from all registered health checks and provides a comprehensive
    /// view of the application's health status. It's typically used by monitoring endpoints and health dashboards.
    /// </remarks>
    public class HealthCheckDto {

        /// <summary>
        /// Gets or sets the overall health status of the application
        /// </summary>
        /// <value>
        /// The aggregated status string (e.g., "Healthy", "Unhealthy", "Degraded") based on all health check results
        /// </value>
        public string? Status { get; set; }

        /// <summary>
        /// Gets or sets the total duration taken to execute all health checks
        /// </summary>
        /// <value>
        /// The formatted duration string representing the time taken to complete all health checks
        /// </value>
        public string? TotalDuration { get; set; }

        /// <summary>
        /// Gets or sets the collection of individual health check results
        /// </summary>
        /// <value>
        /// An enumerable collection of health check items, each representing the result of a specific health check
        /// </value>
        public IEnumerable<HealthCheckItemDto> Entries { get; set; } = [];
    }
}
