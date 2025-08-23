using Newtonsoft.Json;

namespace Myth.ValueProviders {

	internal class InterfaceToConcreteConverter<TInterface, TConcrete> : JsonConverter where TConcrete : TInterface {

		static InterfaceToConcreteConverter( ) {
			// TConcrete should be a subtype of an abstract type, or an implementation of an interface.  If they
			// are identical an infinite recursion could result, so throw an exception.
			if ( typeof( TInterface ) == typeof( TConcrete ) )
				throw new InvalidOperationException( string.Format( "typeof({0}) == typeof({1})", typeof( TInterface ), typeof( TConcrete ) ) );
		}

		public override bool CanConvert( Type objectType ) => objectType == typeof( TInterface );

		public override object ReadJson( JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer ) => serializer.Deserialize( reader, typeof( TConcrete ) );

		public override bool CanWrite => false;

		public override void WriteJson( JsonWriter writer, object value, JsonSerializer serializer ) => throw new NotImplementedException( );
	}
}