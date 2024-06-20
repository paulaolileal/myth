using Microsoft.Extensions.DependencyInjection;
using Myth.Contexts;

namespace Myth.Repository.Test.Extensions {

	internal static class ContextExtensions {

		public static IServiceCollection CreateTestDatabase<TContext>( this IServiceCollection services ) where TContext : BaseContext {
			return services;
		}
	}
}