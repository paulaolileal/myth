using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Myth.Instrumentations.DTOs;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;

namespace Myth.Instrumentations.Controllers {

    /// <summary>
    /// Provides endpoints for application metrics, health checks, and environment information
    /// </summary>
    /// <remarks>
    /// This controller exposes monitoring and telemetry endpoints that can be used by external monitoring systems,
    /// health check probes, and development tools to assess application status and performance.
    /// </remarks>
    [ApiController]
    [Route("[controller]")]
    public class MetricsController : ControllerBase {
        private readonly HealthCheckService _healthCheckService;
        private readonly IHostEnvironment _environment;
        private const string _tag = "Service Metrics";
        private const string _baseDescription = "## Description\n";

        /// <summary>
        /// Initializes a new instance of the MetricsController class
        /// </summary>
        /// <param name="healthCheckService">The health check service for monitoring application dependencies</param>
        /// <param name="environment">The host environment service for retrieving environment information</param>
        /// <exception cref="ArgumentNullException">Thrown when healthCheckService or environment is null</exception>
        public MetricsController(HealthCheckService healthCheckService, IHostEnvironment environment) {
            _healthCheckService = healthCheckService;
            _environment = environment;
        }

        [SwaggerOperation(
            Summary = "Get a prometheus metrics",
            Description = $"{_baseDescription}List all metrics performed on all over aplication by time",
            OperationId = "GetPrometheusMetrics",
            Tags = [_tag]
        )]
        [SwaggerResponse(302, "Redirect to Prometheus route")]
        [HttpGet("Prometheus")]
        public IActionResult GetPrometheusMetrics() => Redirect("/metrics");

        [SwaggerOperation(
            Summary = "Get a health check",
            Description = $"{_baseDescription}Get the current state of application",
            OperationId = "GetHealthCheckMetrics",
            Tags = [_tag]
        )]
        [SwaggerResponse(200, "Service is healthy")]
        [SwaggerResponse(503, "Some part of service is unavailable")]
        /// <summary>
        /// Retrieves the comprehensive health status of the application and all its dependencies
        /// </summary>
        /// <param name="cancellationToken">Cancellation token for the async operation</param>
        /// <returns>An action result containing the health check report with status, duration, and individual check details</returns>
        /// <response code="200">Returns when the application is healthy</response>
        /// <response code="503">Returns when one or more dependencies are unhealthy</response>
        [HttpGet("HealthCheck")]
        public async Task<IActionResult> GetHealthCheckAsync(CancellationToken cancellationToken) {
            var report = await _healthCheckService.CheckHealthAsync(cancellationToken);

            var response = new HealthCheckDto() {
                Status = report.Status.ToString(),
                TotalDuration = report.TotalDuration.TotalSeconds + " seconds",
                Entries = report.Entries.Select(entry => new HealthCheckItemDto {
                    Name = entry.Key,
                    Status = entry.Value.Status.ToString(),
                    Description = entry.Value.Description?.ToString(),
                    Tags = entry.Value.Tags,
                    Duration = entry.Value.Duration.TotalSeconds.ToString() + " seconds",
                    Message = entry.Value.Exception?.Message
                })
            };

            var statusCode = HttpStatusCode.OK;
            if (report.Status != HealthStatus.Healthy)
                statusCode = HttpStatusCode.ServiceUnavailable;

            return StatusCode((int)statusCode, response);
        }

        [ProducesResponseType(typeof(EnvironmentDto), StatusCodes.Status200OK)]
        [SwaggerOperation(
            Summary = "Get environment",
            Description = $"{_baseDescription}Get the current service enviroment",
            OperationId = "GetEnvironmentMetrics",
            Tags = [_tag]
        )]
        [SwaggerResponse(200, "The current service environment")]
        [HttpGet("Environment")]
        public IActionResult GetEnvironment() {
            var response = new EnvironmentDto {
                EnvironmentName = _environment.EnvironmentName,
                IsDevelopment = _environment.IsDevelopment(),
                IsStaging = _environment.IsStaging(),
                IsProduction = _environment.IsProduction(),
            };

            return Ok(response);
        }
    }
}
