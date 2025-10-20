using Microsoft.AspNetCore.Builder;
using Myth.Middlewares;

namespace Myth.Extensions {

	/// <summary>
	/// Extension methods for application builder
	/// </summary>
	public static class ApplicationBuilderExtensions {

		/// <summary>
		/// Adds middleware to handle validation exceptions
		/// </summary>
		public static IApplicationBuilder UseGuard( this IApplicationBuilder app ) {
			return app.UseMiddleware<ValidationExceptionMiddleware>( );
		}
	}
}