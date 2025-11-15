using System.Text.Json;
using System.Text.Json.Serialization;

namespace Myth.ValueProviders;

internal class InterfaceToConcreteConverter<TInterface, TConcrete> : JsonConverter<TInterface>
	where TConcrete : TInterface {

	static InterfaceToConcreteConverter( ) {
		// TConcrete should be a subtype of an abstract type, or an implementation of an interface.  If they
		// are identical an infinite recursion could result, so throw an exception.
		if ( typeof( TInterface ) == typeof( TConcrete ) )
			throw new InvalidOperationException( string.Format( "typeof({0}) == typeof({1})", typeof( TInterface ), typeof( TConcrete ) ) );
	}

	public override TInterface? Read( ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options ) =>
		JsonSerializer.Deserialize<TConcrete>( ref reader, options );

	public override void Write( Utf8JsonWriter writer, TInterface value, JsonSerializerOptions options ) =>
		JsonSerializer.Serialize( writer, value, typeof( TConcrete ), options );
}
