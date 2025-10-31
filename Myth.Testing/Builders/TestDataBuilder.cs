using Bogus;

namespace Myth.Testing.Builders {

	/// <summary>
	/// Base class for building test data with fluent API
	/// </summary>
	/// <typeparam name="T">The type of object to build</typeparam>
	/// <typeparam name="TBuilder">The builder type for fluent chaining</typeparam>
	public abstract class TestDataBuilder<T, TBuilder> where TBuilder : TestDataBuilder<T, TBuilder> {
		protected readonly Faker _faker;
		protected readonly Dictionary<string, object> _overrides;

		/// <summary>
		/// Initializes a new instance of the TestDataBuilder class
		/// </summary>
		/// <param name="faker">The Faker instance for generating test data</param>
		protected TestDataBuilder( Faker faker ) {
			_faker = faker ?? throw new ArgumentNullException( nameof( faker ) );
			_overrides = new Dictionary<string, object>( );
		}

		/// <summary>
		/// Override a property value
		/// </summary>
		/// <param name="propertyName">The property name</param>
		/// <param name="value">The value to set</param>
		/// <returns>The builder instance for fluent chaining</returns>
		public TBuilder With( string propertyName, object value ) {
			_overrides[ propertyName ] = value;

			return ( TBuilder )this;
		}

		/// <summary>
		/// Build the test object
		/// </summary>
		/// <returns>The built test object</returns>
		public abstract T Build( );

		/// <summary>
		/// Build multiple test objects
		/// </summary>
		/// <param name="count">The number of objects to build</param>
		/// <returns>A collection of built test objects</returns>
		public virtual IEnumerable<T> Build( int count ) {
			for ( int i = 0; i < count; i++ )
				yield return Build( );
		}

		/// <summary>
		/// Build a list of test objects
		/// </summary>
		/// <param name="count">The number of objects to build</param>
		/// <returns>A list of built test objects</returns>
		public virtual List<T> BuildList( int count ) {
			return Build( count ).ToList( );
		}

		/// <summary>
		/// Get an override value or return default
		/// </summary>
		/// <typeparam name="TValue">The value type</typeparam>
		/// <param name="propertyName">The property name</param>
		/// <param name="defaultValue">The default value if override not found</param>
		/// <returns>The override value or default</returns>
		protected TValue GetOverrideOrDefault<TValue>( string propertyName, TValue defaultValue ) {
			if ( _overrides.TryGetValue( propertyName, out var value ) && value is TValue typedValue )
				return typedValue;

			return defaultValue;
		}

		/// <summary>
		/// Get an override value or generate using Faker
		/// </summary>
		/// <typeparam name="TValue">The value type</typeparam>
		/// <param name="propertyName">The property name</param>
		/// <param name="fakerGenerator">The Faker generator function</param>
		/// <returns>The override value or generated value</returns>
		protected TValue GetOverrideOrGenerate<TValue>( string propertyName, Func<Faker, TValue> fakerGenerator ) {
			if ( _overrides.TryGetValue( propertyName, out var value ) && value is TValue typedValue )
				return typedValue;

			return fakerGenerator( _faker );
		}
	}
}