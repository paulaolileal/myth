namespace Myth.Instrumentations.DTOs {
    public class HealthCheckItemDto {
        public string? Name { get; set; }
        public string? Status { get; set; }
        public string? Description { get; set; }
        public string? Duration { get; set; }
        public string? Message { get; set; }
        public IEnumerable<string> Tags { get; set; } = [];
    }
}
