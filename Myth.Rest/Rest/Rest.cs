using Microsoft.Extensions.ObjectPool;
using Myth.Interfaces;

namespace Myth.Rest;

/// <summary>
/// Static entry point for REST operations
/// </summary>
public static class Rest {

	private static readonly ObjectPool<RestBuilder> _builderPool =
	   new DefaultObjectPool<RestBuilder>(new RestBuilderPoolPolicy());

	/// <summary>
	/// Create a new REST request builder with fluent interface
	/// </summary>
	/// <returns>REST configuration interface</returns>
	public static IRestBuilder Create() => _builderPool.Get();
}

public class RestBuilderPoolPolicy : IPooledObjectPolicy<RestBuilder> {

	public RestBuilder Create() => new();

	public bool Return(RestBuilder obj) {
		obj.Dispose();

		return true;
	}
}