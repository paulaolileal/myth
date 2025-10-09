namespace Myth.Flow.Interfaces;

/// <summary>
/// Cache configuration for queries
/// </summary>
public sealed class CacheOptions {
	public bool Enabled { get; set; }
	public string? CacheKey { get; set; }
	public TimeSpan Ttl { get; set; } = TimeSpan.FromMinutes( 5 );
	public bool SlidingExpiration { get; set; }
	public Func<object, string>? KeyGenerator { get; set; }
}