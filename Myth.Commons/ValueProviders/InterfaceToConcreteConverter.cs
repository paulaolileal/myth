using System.Text.Json;
using System.Text.Json.Serialization;

namespace Myth.ValueProviders;

internal class InterfaceToConcreteConverter : JsonConverter<object> {
	private readonly Type _interfaceType;
	private readonly Type _concreteType;

	public InterfaceToConcreteConverter( Type interfaceType, Type concreteType ) {
		// TConcrete should be a subtype of an abstract type, or an implementation of an interface.  If they
		// are identical an infinite recursion could result, so throw an exception.
		if ( interfaceType == concreteType )
			throw new InvalidOperationException( string.Format( "typeof({0}) == typeof({1})", interfaceType, concreteType ) );

		_interfaceType = interfaceType;
		_concreteType = concreteType;
	}

	public override bool CanConvert( Type typeToConvert ) => typeToConvert == _interfaceType;

	public override object? Read( ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options ) =>
		JsonSerializer.Deserialize( ref reader, _concreteType, options );

	public override void Write( Utf8JsonWriter writer, object value, JsonSerializerOptions options ) =>
		JsonSerializer.Serialize( writer, value, _concreteType, options );
}
