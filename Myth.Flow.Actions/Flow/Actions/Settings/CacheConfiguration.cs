namespace Myth.Flow.Actions.Settings;

/// <summary>
/// Cache configuration
/// </summary>
public sealed class CacheConfiguration {
	public CacheProviderType ProviderType { get; set; } = CacheProviderType.Memory;
	public TimeSpan DefaultTtl { get; set; } = TimeSpan.FromMinutes( 5 );
	public string? ConnectionString { get; set; }
}
