using Myth.Flow;
using Myth.Interfaces;

namespace Myth.Builders {

	internal sealed class ServiceProviderInitializer : IServiceProviderInitializer {

		public ServiceProviderInitializer( IServiceProvider serviceProvider ) => Pipeline.SetGlobalServiceProvider( serviceProvider );
	}
}