namespace Myth.Extensions {
	internal static class DefaultProvider {
		private static IServiceProvider? _serviceProvider;

		public static IServiceProvider? ServiceProvider {
			get => _serviceProvider;
			set => _serviceProvider = value;
		}

		public static void EnsureProvider( IServiceProvider? sp ) {
			if ( _serviceProvider is null && sp is not null ) 
				ServiceProvider = sp;			
		}
	}
}