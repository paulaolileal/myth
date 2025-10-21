using Myth.Constants;
using Myth.ValueProviders;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Myth.Models;

public class JsonSettings : ICloneable {
	internal bool MinifyResult { get; set; } = false;
	internal bool IgnoreNullValues { get; set; } = false;
	internal IList<JsonConverter> Converters { get; set; } = [ ];
	internal CaseStrategy CaseStrategy { get; set; } = CaseStrategy.CamelCase;
	public Action<JsonSerializerOptions>? OtherSettings { get; set; }

	public JsonSettings( ) {
	}

	internal JsonSettings( JsonSettings settings ) {
		MinifyResult = settings.MinifyResult;
		IgnoreNullValues = settings.IgnoreNullValues;
		Converters = settings.Converters;
		CaseStrategy = settings.CaseStrategy;
		OtherSettings = settings.OtherSettings;
	}

	/// <summary>
	/// Should ignore null values on object
	/// </summary>
	public JsonSettings IgnoreNull( ) {
		IgnoreNullValues = true;

		return this;
	}

	/// <summary>
	/// The case strategy to be used in serialization
	/// </summary>
	public JsonSettings UseCaseStrategy( CaseStrategy strategy ) {
		CaseStrategy = strategy;

		return this;
	}

	/// <summary>
	/// If the result should be minified
	/// </summary>
	public JsonSettings Minify( ) {
		MinifyResult = true;

		return this;
	}

	/// <summary>
	/// Add a interface converter to concrete type for serialization/deserialization
	/// </summary>
	/// <typeparam name="TInterface">The interface of type</typeparam>
	/// <typeparam name="TConcrete">The type of concrete</typeparam>
	public JsonSettings UseInterfaceConverter<TInterface, TConcrete>( ) where TConcrete : TInterface {
		Converters.Add( new InterfaceToConcreteConverter<TInterface, TConcrete>( ) );

		return this;
	}

	/// <summary>
	/// Add a interface converter to concrete type for serialization/deserialization
	/// </summary>
	/// <typeparam name="TInterface">The interface of type</typeparam>
	/// <typeparam name="TConcrete">The type of concrete</typeparam>
	public JsonSettings UseInterfaceConverter( Type interfaceType, Type concreteType ) {
		Converters.Add( new InterfaceToConcreteConverter( interfaceType, concreteType ) );

		return this;
	}

	/// Add a custom converter to concrete type for serialization/deserialization
	/// <param name="converter">The converter of type</typeparam>
	public JsonSettings UseCustomConverter( JsonConverter converter ) {
		Converters.Add( converter );

		return this;
	}

	public object Clone( ) => new JsonSettings( this );

	public JsonSettings Copy( ) => ( JsonSettings )Clone( );

	/// <summary>
	/// Other settings on base serializer settings
	/// </summary>
}