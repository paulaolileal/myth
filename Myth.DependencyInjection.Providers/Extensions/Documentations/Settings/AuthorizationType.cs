namespace Myth.Extensions.Swagger.Settings {

	public partial class SwaggerSettings {

		internal enum AuthorizationType {
			None,
			Bearer,
			Basic,
			ApiKey
		}
	}
}