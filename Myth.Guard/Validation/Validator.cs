using Myth.Builder;
using Myth.Exceptions;
using Myth.Guard;
using Myth.Interfaces;
using Myth.Models;
using Myth.ServiceProvider;

namespace Myth.Validation;

/// <summary>
/// Default implementation of IValidator
/// </summary>
internal sealed class Validator( IServiceProvider serviceProvider ) : IValidator {
	private readonly IServiceProvider _serviceProvider = serviceProvider;

	/// <summary>
	/// Validates an entity and throws exception on failure
	/// </summary>
	public async Task ValidateAsync<T>( T entity, ValidationContextKey? context = null, CancellationToken cancellationToken = default )
		where T : class {
		ArgumentNullException.ThrowIfNull( entity );
		var result = await ValidateAndReturnAsync( entity, context, cancellationToken );

		if ( !result.IsValid ) {
			throw new ValidationException( result );
		}
	}

	/// <summary>
	/// Validates an entity and returns the result
	/// </summary>
	public async Task<ValidationResult> ValidateAndReturnAsync<T>( T entity, ValidationContextKey? context = null, CancellationToken cancellationToken = default )
		where T : class {
		if ( entity is not IValidatable<T> validatable ) {
			throw new InvalidOperationException( $"Type {typeof( T ).Name} does not implement IValidatable<{typeof( T ).Name}>" );
		}

		var builder = new ValidationBuilder<T>( );
		validatable.Validate( builder, context );

		var rules = builder.GetRules( context );
		var result = new ValidationResult( );

		foreach ( var fieldValidation in rules ) {
			var shouldStop = false;

			foreach ( var rule in fieldValidation.Rules ) {
				if ( shouldStop )
					break;

				cancellationToken.ThrowIfCancellationRequested( );

				var ruleContext = new RuleContext<object>(
					value: GetFieldValue( entity, fieldValidation.FieldName ),
					fieldName: fieldValidation.FieldName,
					serviceProvider: MythServiceProvider.GetOrFallback( _serviceProvider ),
					cancellationToken: cancellationToken,
					entity: entity
				);

				var error = await rule.ValidateAsync( ruleContext );

				if ( error != null ) {
					result.AddError( error );

					if ( rule.StopOnFailure ) {
						shouldStop = true;
					}
				}
			}
		}

		return result;
	}

	private static object GetFieldValue<T>( T entity, string fieldName ) where T : class {
		var property = typeof( T ).GetProperty( fieldName );
		if ( property != null ) {
			return property.GetValue( entity )!;
		}

		var field = typeof( T ).GetField( fieldName );
		if ( field != null ) {
			return field.GetValue( entity )!;
		}

		return null!;
	}
}
