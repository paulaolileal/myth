using Bogus;

namespace Myth.Mocks;

/// <summary>
/// Provides a base class for generating mock model entities for testing or data seeding purposes.
/// </summary>
/// <remarks>
/// This abstract class serves as a foundation for creating type-specific mock data generators.
/// Derived classes must implement the generation logic for their specific model types using the
/// provided Faker instance. The class supports both single and batch entity generation with
/// optional metadata parameters for customizing the generated data.
/// </remarks>
/// <typeparam name="TModel">The type of the model entity to generate. Must be a reference type.</typeparam>
/// <param name="faker">The Faker instance used to generate randomized data for model entities.</param>
public abstract class ModelMock<TModel>( Faker faker )
	where TModel : class {
	protected readonly Faker _faker = faker;

	/// <summary>
	/// Asynchronously generates a collection of model instances.
	/// </summary>
	/// <param name="amount">The number of model instances to generate. Must be greater than zero.</param>
	/// <returns>A task that represents the asynchronous operation. The task result contains an enumerable collection of
	/// generated model instances.</returns>
	public abstract IEnumerable<TModel> Generate( int amount, IDictionary<string, object>? metadata = null );

	/// <summary>
	/// Asynchronously generates a single instance of the model.
	/// </summary>
	/// <returns>A task that represents the asynchronous operation. The task result contains the generated model instance.</returns>
	public virtual TModel Generate( IDictionary<string, object>? metadata = null ) => ( Generate( 1, metadata ) ).First( );
}
