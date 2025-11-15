using System.Threading.Tasks;
using Myth.Flow.Test.Models;

namespace Myth.Flow.Test.Interfaces;

public interface IUserRepository {

	Task<bool> EmailExistsAsync( string email );

	Task<User> CreateUserAsync( User user );
}
