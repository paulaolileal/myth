using Newtonsoft.Json;

namespace Myth.ValueProviders {

	internal class InterfaceToConcreteConverter : JsonConverter {
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

		public override bool CanConvert( Type objectType ) => objectType == _interfaceType;

		public override object ReadJson( JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer ) => serializer.Deserialize( reader, _concreteType );

		public override bool CanWrite => false;

		public override void WriteJson( JsonWriter writer, object value, JsonSerializer serializer ) => throw new NotImplementedException( );
	}
}