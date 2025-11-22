namespace Myth.Models;

/// <summary>
/// Internal rule execution context
/// </summary>
public sealed class RuleContext<T>( T value, string fieldName, IServiceProvider serviceProvider, CancellationToken cancellationToken, object? entity = null ) {
	public T Value { get; init; } = value;
	public string FieldName { get; init; } = fieldName;
	public IServiceProvider ServiceProvider { get; init; } = serviceProvider;
	public CancellationToken CancellationToken { get; init; } = cancellationToken;
	public object? Entity { get; init; } = entity;
}
