using Myth.Morph;

namespace Myth.Interfaces {

	/// <summary>
	/// Defines a contract for objects that can be created from a specific source type.
	/// This interface enables instance-based mapping configuration where the destination object itself
	/// defines how it should be created from the source type.
	/// Ideal for DTOs that know how to create themselves from entities.
	/// </summary>
	/// <typeparam name="TSource">The type that this object can be created from.</typeparam>
	public interface IMorphableFrom<TSource> {

		/// <summary>
		/// Configures bindings for creating this destination object from the specified source type.
		/// </summary>
		/// <remarks>
		/// Use this method to specify how properties or fields from the source type should be mapped to
		/// this destination object. The <paramref name="schema"/> parameter provides methods and options for customizing the
		/// mapping behavior, including property binding, async binding, and property ignoring.
		///
		/// This method is called by the Morph system during the transformation process to configure
		/// the mapping rules specific to this destination type. Services can be accessed via MythServiceProvider.
		/// </remarks>
		/// <param name="schema">A <see cref="Schema{TSource}"/> instance used to define the mapping rules and bindings for the
		/// source type.</param>
		void MorphFrom( Schema<TSource> schema );
	}
}