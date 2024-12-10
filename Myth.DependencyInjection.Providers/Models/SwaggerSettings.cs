using Swashbuckle.AspNetCore.SwaggerGen;

namespace Myth.Models;

public class SwaggerSettings( SwaggerGenOptions options ) {

	/// <summary>
	/// The title of API
	/// </summary>
	public string Title { get; set; } = null!;

	/// <summary>
	/// The description of API. The purpose of API
	/// </summary>
	public string Description { get; set; } = null!;

	/// <summary>
	/// Text to show on deprecated versions
	/// </summary>
	public string DeprecatedDescription { get; set; } = "This version of API is deprecated!";

	/// <summary>
	/// The name of authors and contact point
	/// </summary>
	public string ContactName { get; set; } = null!;

	/// <summary>
	/// The email of authors and contact point
	/// </summary>
	public string ContactEmail { get; set; } = null!;

	/// <summary>
	/// The url of application and docs
	/// </summary>
	public string ContactUrl { get; set; } = null!;

	/// <summary>
	/// Other options of base Swagger
	/// </summary>
	public SwaggerGenOptions Options { get; } = options;
}