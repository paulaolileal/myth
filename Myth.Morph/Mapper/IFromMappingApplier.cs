namespace Myth.Morph;

/// <summary>
/// Internal interface that allows <see cref="Schema{TDestination}"/> to invoke
/// <see cref="FromMappingExecutor{TDestination}"/> without reflection when the
/// destination type is only known at runtime.
/// Implemented explicitly by <see cref="FromMappingExecutor{TDestination}"/>.
/// </summary>
internal interface IFromMappingApplier {
	void ApplyMapping(
		object source,
		object destination,
		IServiceProvider serviceProvider,
		HashSet<string> manuallyMappedProps,
		HashSet<string> ignoredProperties );
}
