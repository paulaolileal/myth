using Myth.Contexts;
using Myth.Interfaces.Repositories.EntityFramework;
using Myth.Repositories.EntityFramework.Base;

namespace Myth.Repositories.EntityFramework;

public class UnitOfWorkRepository<TContext>( TContext context ) : BaseUnitOfWorkRepository( context ), IUnitOfWorkRepository where TContext : BaseContext {
}
