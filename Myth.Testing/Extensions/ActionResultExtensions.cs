using Microsoft.AspNetCore.Mvc;
using Myth.Extensions;

namespace Myth.Testing.Extensions {

	public static class ActionResultExtensions {

		/// <summary>
		/// Get the body of controller result
		/// </summary>
		/// <typeparam name="T">The type to cast the result</typeparam>
		/// <param name="response"></param>
		/// <returns></returns>
		/// <exception cref="Exception"></exception>
		public static T? GetAs<T>( this IActionResult response ) where T : class {
			if ( response is ObjectResult objectResultResponse )
				return objectResultResponse?.Value as T;

			if ( response is ContentResult contentResultResponse )
				return contentResultResponse.Content?.FromJson<T>( );

			throw new Exception( "Type is not valid!" );
		}
	}
}