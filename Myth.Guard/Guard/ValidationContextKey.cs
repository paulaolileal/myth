namespace Myth.Guard;

/// <summary>
/// Represents a validation context key that determines which set of validation rules should be applied.
/// Context keys enable context-aware validation where the same entity can have different validation rules
/// depending on the operation being performed (Create, Update, Delete, etc.).
/// </summary>
/// <remarks>
/// Context keys allow entities to define different validation rules for different scenarios.
/// For example, an entity might require certain fields during creation but allow them to be optional during updates,
/// or it might have stricter validation rules during deletion to prevent accidental data loss.
///
/// <para>
/// Use predefined context keys for common operations, or create custom contexts using the <see cref="Custom(string)"/> method
/// for specific business scenarios.
/// </para>
/// </remarks>
/// <param name="Key">The string identifier for the validation context.</param>
/// <example>
/// <code>
/// // Using predefined contexts
/// await validator.ValidateAsync(user, ValidationContextKey.Create);
/// await validator.ValidateAsync(user, ValidationContextKey.Update);
///
/// // Using custom context
/// var customContext = ValidationContextKey.Custom("BulkImport");
/// await validator.ValidateAsync(user, customContext);
///
/// // Implicit conversion from string
/// await validator.ValidateAsync(user, "MyCustomContext");
/// </code>
/// </example>
public readonly record struct ValidationContextKey( string Key ) {
	/// <summary>
	/// The default validation context used when no specific context is provided.
	/// </summary>
	public static readonly ValidationContextKey Default = new( "Default" );

	/// <summary>
	/// Validation context for entity creation operations.
	/// Typically used for validating new entities before they are persisted to the database.
	/// </summary>
	public static readonly ValidationContextKey Create = new( "Create" );

	/// <summary>
	/// Validation context for entity update operations.
	/// Used when modifying existing entities, which may have different validation requirements than creation.
	/// </summary>
	public static readonly ValidationContextKey Update = new( "Update" );

	/// <summary>
	/// Validation context for entity deletion operations.
	/// Used to ensure entities can be safely deleted without violating business rules.
	/// </summary>
	public static readonly ValidationContextKey Delete = new( "Delete" );

	/// <summary>
	/// Validation context for field-based query operations.
	/// Used when validating input parameters for queries that search by specific fields.
	/// </summary>
	public static readonly ValidationContextKey GetByField = new( "GetByField" );

	/// <summary>
	/// Validation context for operations that retrieve all entities.
	/// Used when validating pagination or filtering parameters for bulk retrieval operations.
	/// </summary>
	public static readonly ValidationContextKey GetAll = new( "GetAll" );

	/// <summary>
	/// Validation context for search operations.
	/// Used when validating search criteria and parameters for entity search functionality.
	/// </summary>
	public static readonly ValidationContextKey Search = new( "Search" );

	/// <summary>
	/// Validation context for entity activation operations.
	/// Used when validating that an entity can be safely activated or enabled.
	/// </summary>
	public static readonly ValidationContextKey Activate = new( "Activate" );

	/// <summary>
	/// Validation context for entity deactivation operations.
	/// Used when validating that an entity can be safely deactivated or disabled.
	/// </summary>
	public static readonly ValidationContextKey Deactivate = new( "Deactivate" );

	/// <summary>
	/// Creates a custom validation context key with the specified identifier.
	/// </summary>
	/// <param name="key">The string identifier for the custom context. Must not be null.</param>
	/// <returns>A new <see cref="ValidationContextKey"/> with the specified key.</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="key"/> is null.</exception>
	/// <example>
	/// <code>
	/// var bulkImportContext = ValidationContextKey.Custom("BulkImport");
	/// var adminContext = ValidationContextKey.Custom("AdminOperation");
	/// </code>
	/// </example>
	public static ValidationContextKey Custom( string key ) {
		ArgumentNullException.ThrowIfNull( key );

		return new( key );
	}

	/// <summary>
	/// Returns the string representation of the validation context key.
	/// </summary>
	/// <returns>The key identifier as a string.</returns>
	public override string ToString( ) => Key;

	/// <summary>
	/// Implicitly converts a string to a <see cref="ValidationContextKey"/>.
	/// This allows for convenient creation of context keys from string literals.
	/// </summary>
	/// <param name="key">The string to convert to a validation context key.</param>
	/// <returns>A new <see cref="ValidationContextKey"/> with the specified key.</returns>
	/// <example>
	/// <code>
	/// ValidationContextKey context = "MyCustomContext"; // Implicit conversion
	/// await validator.ValidateAsync(entity, "DirectStringContext");
	/// </code>
	/// </example>
	public static implicit operator ValidationContextKey( string key ) => new( key );
}
