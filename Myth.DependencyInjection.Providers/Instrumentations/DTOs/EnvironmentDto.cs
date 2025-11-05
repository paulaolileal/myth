namespace Myth.Instrumentations.DTOs {

    /// <summary>
    /// Represents environment information for instrumentation and monitoring purposes
    /// </summary>
    /// <remarks>
    /// This DTO is used to provide environment context in telemetry data and health check responses.
    /// It helps identify which environment (Development, Staging, Production) the application is running in.
    /// </remarks>
    public class EnvironmentDto {

        /// <summary>
        /// Gets or sets the name of the current environment
        /// </summary>
        /// <value>
        /// The environment name (e.g., "Development", "Staging", "Production") or null if not set
        /// </value>
        public string? EnvironmentName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the application is running in development environment
        /// </summary>
        /// <value>
        /// true if the environment is development; otherwise, false
        /// </value>
        public bool IsDevelopment { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the application is running in staging environment
        /// </summary>
        /// <value>
        /// true if the environment is staging; otherwise, false
        /// </value>
        public bool IsStaging { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the application is running in production environment
        /// </summary>
        /// <value>
        /// true if the environment is production; otherwise, false
        /// </value>
        public bool IsProduction { get; set; }
    }
}
