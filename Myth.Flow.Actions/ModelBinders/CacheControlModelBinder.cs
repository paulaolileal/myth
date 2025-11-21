using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Myth.Flow.Actions.ValueObjects;

namespace Myth.Flow.Actions.ModelBinders;

/// <summary>
/// Model binder for CacheControl that automatically parses Cache-Control headers
/// </summary>
public class CacheControlModelBinder( ILogger<CacheControlModelBinder> logger ) : IModelBinder {
	private readonly ILogger<CacheControlModelBinder> _logger = logger;

	public Task BindModelAsync( ModelBindingContext bindingContext ) {
		if ( bindingContext == null ) {
			throw new ArgumentNullException( nameof( bindingContext ) );
		}

		// Try to get Cache-Control header
		var request = bindingContext.HttpContext.Request;
		if ( !request.Headers.TryGetValue( "Cache-Control", out var headerValues ) ) {
			bindingContext.Result = ModelBindingResult.Success( null );
			return Task.CompletedTask;
		}

		var headerValue = headerValues.FirstOrDefault( );
		var cacheControl = CacheControl.FromModelBinding( headerValue );

		// Log warning if parse failed but header was present
		if ( !string.IsNullOrWhiteSpace( headerValue ) && !cacheControl.IsValid ) {
			_logger.LogWarning(
				"Invalid Cache-Control header '{HeaderValue}' received. " +
				"Expected format examples: 'no-cache', 'max-age=3600', 'public, max-age=1800', 'private, max-age=300'. " +
				"Request will proceed without caching. Endpoint: {Endpoint}",
				headerValue,
				bindingContext.ActionContext.ActionDescriptor.DisplayName );
		}

		bindingContext.Result = ModelBindingResult.Success( cacheControl );
		return Task.CompletedTask;
	}
}

/// <summary>
/// Model binder provider for CacheControl
/// </summary>
public class CacheControlModelBinderProvider : IModelBinderProvider {
	public IModelBinder? GetBinder( ModelBinderProviderContext context ) {
		if ( context.Metadata.ModelType == typeof( CacheControl ) ) {
			return new CacheControlModelBinder(
				context.Services.GetRequiredService<ILogger<CacheControlModelBinder>>( ) );
		}

		return null;
	}
}
