using Myth.Guard;
using Myth.Interfaces;
using Myth.Rules.Base;

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
		public void For<TValue>( TValue value, Action<FluentRuleBuilder<TValue>> configure, [System.Runtime.CompilerServices.CallerArgumentExpression( "value" )] string fieldName = "" ) {
			var cleanFieldName = CleanFieldName( fieldName );
			var builder = CreateRuleBuilder<TValue>( cleanFieldName );
			configure( builder );

			var validation = new FieldValidation {
				FieldName = cleanFieldName,
				Rules = builder.GetRules( ).Cast<IValidationRule>( ).ToList( )
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

		private FluentRuleBuilder<TValue> CreateRuleBuilder<TValue>( string fieldName ) {
			var fieldBuilder = new FieldRuleBuilder<TValue>( fieldName );
			return new FluentRuleBuilder<TValue>( fieldBuilder );
		}
	}
}