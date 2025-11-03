namespace Myth.Extensions.Swagger.Settings {

	/// <summary>
	/// Configuration for persistent cache functionality
	/// </summary>
	internal class CacheSettings {

		/// <summary>
		/// Enable cache persistence across browser sessions
		/// </summary>
		public bool EnablePersistence { get; set; } = true;

		/// <summary>
		/// Cache expiration time in minutes (0 = no expiration)
		/// </summary>
		public int ExpirationMinutes { get; set; } = 60;

		/// <summary>
		/// Enable request history per endpoint
		/// </summary>
		public bool EnableHistory { get; set; } = true;

		/// <summary>
		/// Maximum number of history entries per endpoint
		/// </summary>
		public int MaxHistoryEntries { get; set; } = 50;

		/// <summary>
		/// Cache storage type
		/// </summary>
		public CacheStorageType StorageType { get; set; } = CacheStorageType.LocalStorage;

		/// <summary>
		/// Cache key prefix to avoid conflicts
		/// </summary>
		public string KeyPrefix { get; set; } = "swagger_cache_";

		/// <summary>
		/// Enable cache for request parameters
		/// </summary>
		public bool CacheParameters { get; set; } = true;

		/// <summary>
		/// Enable cache for request bodies
		/// </summary>
		public bool CacheBodies { get; set; } = true;

		/// <summary>
		/// Enable cache for headers
		/// </summary>
		public bool CacheHeaders { get; set; } = true;

		/// <summary>
		/// Enable automatic cache cleanup on expiration
		/// </summary>
		public bool AutoCleanup { get; set; } = true;
	}

	/// <summary>
	/// Cache storage types
	/// </summary>
	internal enum CacheStorageType {

		/// <summary>
		/// Use browser localStorage (persists across sessions)
		/// </summary>
		LocalStorage,

		/// <summary>
		/// Use browser sessionStorage (cleared on browser close)
		/// </summary>
		SessionStorage,

		/// <summary>
		/// Use in-memory storage (cleared on page refresh)
		/// </summary>
		Memory
	}
}