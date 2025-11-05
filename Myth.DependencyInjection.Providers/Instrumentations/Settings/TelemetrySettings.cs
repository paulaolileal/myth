using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace Myth.Instrumentations.Settings {
    public class TelemetrySettings {

        public Action<MeterProviderBuilder>? MetricsProviderBuilder { get; internal set; }
        public Action<TracerProviderBuilder>? TracingProviderBuilder { get; internal set; }


        internal string ApplicationName { get; set; } = null!;
        public TelemetrySettings UseApplicationName(string applicationName) {
            ApplicationName = applicationName;
            return this;
        }

        internal string OpenTelemetryUrl { get; set; } = null!;
        public TelemetrySettings UseServerUrl(string serverUrl) {
            OpenTelemetryUrl = serverUrl;
            return this;
        }
    }
}
