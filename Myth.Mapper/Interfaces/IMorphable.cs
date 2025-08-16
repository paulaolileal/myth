using Myth.Morph;

namespace Myth.Interfaces {

	public interface IMorphable<TDestination> {

		/// <summary>
		/// Configures bindings for mapping source objects to the specified destination type.
		/// </summary>
		/// <remarks>Use this method to specify how properties or fields from the source object should be mapped to
		/// the destination type. The <paramref name="builder"/> parameter provides methods and options for customizing the
		/// mapping behavior.</remarks>
		/// <param name="builder">A <see cref="Schema{TDestination}"/> instance used to define the mapping rules and bindings for the
		/// destination type.</param>
		void MorphTo( Schema<TDestination> builder );
	}
}