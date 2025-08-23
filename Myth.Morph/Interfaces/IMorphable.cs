using Myth.Morph;

namespace Myth.Interfaces {

	/// <summary>
	/// Defines a contract for objects that can be morphed (transformed) into a specific destination type.
	/// This interface enables instance-based mapping configuration where the source object itself
	/// defines how it should be mapped to the destination type.
	/// </summary>
	/// <typeparam name="TDestination">The type that this object can be morphed into.</typeparam>
	public interface IMorphable<TDestination> {

		/// <summary>
		/// Configures bindings for mapping source objects to the specified destination type.
		/// </summary>
		/// <remarks>
		/// Use this method to specify how properties or fields from the source object should be mapped to
		/// the destination type. The <paramref name="schema"/> parameter provides methods and options for customizing the
		/// mapping behavior, including property binding, async binding, and property ignoring.
		///
		/// This method is called by the Morph system during the transformation process to configure
		/// the mapping rules specific to this source type.
		/// </remarks>
		/// <param name="schema">A <see cref="Schema{TDestination}"/> instance used to define the mapping rules and bindings for the
		/// destination type.</param>
		void MorphTo( Schema<TDestination> schema );
	}
}