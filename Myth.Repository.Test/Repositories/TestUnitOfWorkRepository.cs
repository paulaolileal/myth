using Myth.Repositories.EntityFramework;
using Myth.Repository.Test.Contexts;

namespace Myth.Repository.Test.Repositories;

internal class TestUnitOfWorkRepository : UnitOfWorkRepository<ContextTest> {

	public TestUnitOfWorkRepository( ContextTest context ) : base( context ) {
	}
}
