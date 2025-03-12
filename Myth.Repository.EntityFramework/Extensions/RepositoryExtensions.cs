using Microsoft.Extensions.DependencyInjection;
using Myth.Interfaces.Repositories.EntityFramework;
using Myth.Repositories.EntityFramework;

namespace Myth.Extensions {

	public static class RepositoryExtensions {

		public static IServiceCollection AddUnitOfWork( this IServiceCollection services ) {
			services.AddScoped<IUnitOfWorkRepository, UnitOfWorkRepository>( );

			return services;
		}
	}
}