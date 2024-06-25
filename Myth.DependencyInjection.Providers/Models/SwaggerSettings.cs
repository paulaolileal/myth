using Swashbuckle.AspNetCore.SwaggerGen;

namespace Myth.DependencyInjection.Providers.Models {

	public class SwaggerSettings {
		public string Title { get; set; } = null!;
		public string Description { get; set; } = null!;
		public string DeprecatedDescription { get; set; } = "This version of API is deprecated!";
		public string ContactName { get; set; } = null!;
		public string ContactEmail { get; set; } = null!;
		public string ContactUrl { get; set; } = null!;
		public SwaggerGenOptions Options { get; }

		public SwaggerSettings( SwaggerGenOptions options ) {
			Options = options;
		}
	}
}