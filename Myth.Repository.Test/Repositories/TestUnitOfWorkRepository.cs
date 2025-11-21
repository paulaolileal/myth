using Myth.Repositories.EntityFramework;
using Myth.Repository.Test.Contexts;

namespace Myth.Repository.Test.Repositories;

internal class TestUnitOfWorkRepository( ContextTest context ) : UnitOfWorkRepository<ContextTest>( context ) {
}
