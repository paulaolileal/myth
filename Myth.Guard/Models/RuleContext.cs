namespace Myth.Models; 

/// <summary>
/// Internal rule execution context
/// </summary>
public sealed class RuleContext<T> {
	public T Value { get; init; }
	public string FieldName { get; init; } = string.Empty;
	public IServiceProvider ServiceProvider { get; init; }
	public CancellationToken CancellationToken { get; init; }
	public object? Entity { get; init; }

	public RuleContext( T value, string fieldName, IServiceProvider serviceProvider, CancellationToken cancellationToken, object? entity = null ) {
		Value = value;
		FieldName = fieldName;
		ServiceProvider = serviceProvider;
		CancellationToken = cancellationToken;
		Entity = entity;
	}
}
