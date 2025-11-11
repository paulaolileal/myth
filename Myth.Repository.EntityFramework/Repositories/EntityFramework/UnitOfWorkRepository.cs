using Myth.Contexts;
using Myth.Interfaces.Repositories.EntityFramework;
using Myth.Repositories.EntityFramework.Base;

namespace Myth.Repositories.EntityFramework;

public class UnitOfWorkRepository<TContext> : BaseUnitOfWorkRepository, IUnitOfWorkRepository where TContext : BaseContext {

	public UnitOfWorkRepository( TContext context ) : base( context ) {
	}
}
