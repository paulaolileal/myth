using Microsoft.Extensions.ObjectPool;

namespace Myth.Rest;

public class RestBuilderPoolPolicy : IPooledObjectPolicy<RestBuilder> {

	public RestBuilder Create( ) => new( );

	public bool Return( RestBuilder obj ) {
		obj.Dispose( );

		return true;
	}
}