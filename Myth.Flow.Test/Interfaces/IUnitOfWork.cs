using System.Threading.Tasks;

namespace Myth.Flow.Test.Interfaces {

	public interface IUnitOfWork {

		Task CommitAsync( );

		Task RollbackAsync( );

		Task BeginTransactionAsync( );
	}
}