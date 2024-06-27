using Swashbuckle.AspNetCore.SwaggerGen;

namespace Myth.Models;

public class SwaggerSettings( SwaggerGenOptions options ) {
	public string Title { get; set; } = null!;
	public string Description { get; set; } = null!;
	public string DeprecatedDescription { get; set; } = "This version of API is deprecated!";
	public string ContactName { get; set; } = null!;
	public string ContactEmail { get; set; } = null!;
	public string ContactUrl { get; set; } = null!;
	public SwaggerGenOptions Options { get; } = options;
}