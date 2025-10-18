using Myth.Guard;
using Myth.Interfaces;
using Myth.Rules.Base;
using Myth.Rules.Boooleans;
using Myth.Rules.Collections;
using Myth.Rules.Dates;
using Myth.Rules.DateTimes;
using Myth.Rules.Enums;
using Myth.Rules.Numerics;
using Myth.Rules.Strings;

namespace Myth.Builder {

	/// <summary>
	/// Builds validation rules for an entity
	/// </summary>
	public sealed class ValidationBuilder<TEntity> where TEntity : class {
		private readonly Dictionary<ValidationContextKey, List<FieldValidation>> _contextRules = new( );
		private readonly List<FieldValidation> _globalRules = new( );
		private ValidationContextKey? _currentContext;

		/// <summary>
		/// Defines rules for a specific context
		/// </summary>
		public void InContext( ValidationContextKey context, Action<ValidationBuilder<TEntity>> configure ) {
			if ( !_contextRules.ContainsKey( context ) )
				_contextRules[ context ] = new List<FieldValidation>( );

			_currentContext = context;
			configure( this );
			_currentContext = null;
		}

		/// <summary>
		/// Defines validation rules for a field
		/// </summary>
		public void For<TValue>( TValue value, Action<dynamic> configure, [System.Runtime.CompilerServices.CallerArgumentExpression( "value" )] string fieldName = "" ) {
			var cleanFieldName = CleanFieldName( fieldName );
			var builder = CreateRuleBuilder<TValue>( cleanFieldName );
			configure( builder );

			var validation = new FieldValidation {
				FieldName = cleanFieldName,
				Rules = builder.GetRules( ).Cast<IValidationRule<object>>( ).ToList( )
			};

			if ( _currentContext.HasValue ) {
				_contextRules[ _currentContext.Value ].Add( validation );
			} else {
				_globalRules.Add( validation );
			}
		}

		internal List<FieldValidation> GetRules( ValidationContextKey? context ) {
			var rules = new List<FieldValidation>( _globalRules );

			if ( context.HasValue && _contextRules.TryGetValue( context.Value, out var contextRules ) ) {
				rules.AddRange( contextRules );
			}

			return rules;
		}

		private static string CleanFieldName( string fieldName ) {
			var lastDot = fieldName.LastIndexOf( '.' );
			return lastDot >= 0 ? fieldName.Substring( lastDot + 1 ) : fieldName;
		}

		private dynamic CreateRuleBuilder<TValue>( string fieldName ) {
			var builder = new FieldRuleBuilder<TValue>( fieldName );

			return typeof( TValue ) switch {
				Type t when t == typeof( string ) => new StringRules( builder as FieldRuleBuilder<string> ),
				Type t when t == typeof( int ) => new NumericRules<int>( builder as FieldRuleBuilder<int> ),
				Type t when t == typeof( long ) => new NumericRules<long>( builder as FieldRuleBuilder<long> ),
				Type t when t == typeof( short ) => new NumericRules<short>( builder as FieldRuleBuilder<short> ),
				Type t when t == typeof( double ) => new NumericRules<double>( builder as FieldRuleBuilder<double> ),
				Type t when t == typeof( decimal ) => new NumericRules<decimal>( builder as FieldRuleBuilder<decimal> ),
				Type t when t == typeof( float ) => new NumericRules<float>( builder as FieldRuleBuilder<float> ),
				Type t when t == typeof( bool ) => new BooleanRules( builder as FieldRuleBuilder<bool> ),
				Type t when t == typeof( DateTime ) => new DateTimeRules( builder as FieldRuleBuilder<DateTime> ),
				Type t when t == typeof( DateOnly ) => new DateOnlyRules( builder as FieldRuleBuilder<DateOnly> ),
				Type t when t.IsEnum => CreateEnumRules( t, builder ),
				Type t when IsGenericCollection( t ) => CreateCollectionRules( t, builder ),
				_ => new RuleConfigurator<TValue>( builder )
			};
		}

		private static dynamic CreateEnumRules( Type enumType, object builder ) {
			var enumRulesType = typeof( EnumRules<> ).MakeGenericType( enumType );
			return Activator.CreateInstance( enumRulesType, builder )!;
		}

		private static dynamic CreateCollectionRules( Type collectionType, object builder ) {
			var itemType = collectionType.GetGenericArguments( )[ 0 ];
			var collectionRulesType = typeof( CollectionRules<> ).MakeGenericType( itemType );
			return Activator.CreateInstance( collectionRulesType, builder )!;
		}

		private static bool IsGenericCollection( Type type ) {
			return type.IsGenericType &&
				   type.GetGenericTypeDefinition( ) == typeof( IEnumerable<> ) ||
				   type.GetInterfaces( ).Any( i => i.IsGenericType &&
												i.GetGenericTypeDefinition( ) == typeof( IEnumerable<> ) );
		}
	}
}