using System.Collections.Concurrent;
using Myth.Interfaces;

namespace Myth.Morph;

/// <summary>
/// Holds <see cref="IMorphRegistrationBootstrap"/> instances registered by generated code.
/// Populated via <c>[ModuleInitializer]</c> before <c>AddMorph()</c> runs, allowing
/// <see cref="Myth.Extensions.ServiceCollectionExtensions.AddMorph"/> to skip assembly scanning
/// when generated bootstraps are present.
/// </summary>
/// <remarks>
/// Supports multiple bootstraps so that different assemblies in the same application
/// can each contribute their own generated registrations.
/// </remarks>
public static class GeneratedBootstrapRegistry {
	private static readonly ConcurrentBag<IMorphRegistrationBootstrap> _bootstraps = new( );

	/// <summary>
	/// Registers a generated bootstrap. Typically called from a <c>[ModuleInitializer]</c>.
	/// </summary>
	public static void Register( IMorphRegistrationBootstrap bootstrap ) {
		ArgumentNullException.ThrowIfNull( bootstrap );
		_bootstraps.Add( bootstrap );
	}

	/// <summary>Returns all registered bootstraps.</summary>
	public static IReadOnlyCollection<IMorphRegistrationBootstrap> GetAll( )
		=> _bootstraps.ToList( ).AsReadOnly( );

	/// <summary>Returns <c>true</c> when at least one bootstrap has been registered.</summary>
	public static bool HasBootstraps => !_bootstraps.IsEmpty;

	/// <summary>Clears all registered bootstraps. For testing purposes only.</summary>
	internal static void Clear( ) {
		while ( _bootstraps.TryTake( out _ ) ) { }
	}
}
