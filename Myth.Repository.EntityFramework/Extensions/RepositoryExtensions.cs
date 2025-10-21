using Microsoft.Extensions.DependencyInjection;
using Myth.Interfaces.Repositories.EntityFramework;
using Myth.Repositories.EntityFramework;

namespace Myth.Extensions {

	public static class RepositoryExtensions {

		/// <summary>
		/// Registers a Unit of Work repository implementation in the dependency injection container
		/// </summary>
		/// <typeparam name="TUnitOfWork">The concrete implementation of UnitOfWorkRepository to register</typeparam>
		/// <param name="services">The service collection to add the registration to</param>
		/// <returns>The service collection for method chaining</returns>
		public static IServiceCollection AddUnitOfWork<TUnitOfWork>( this IServiceCollection services ) where TUnitOfWork : UnitOfWorkRepository{
			services.AddScoped<IUnitOfWorkRepository, TUnitOfWork>( );

			return services;
		}
	}
}