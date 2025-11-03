using Myth.Interfaces.Repositories.Base;
using Myth.Repository.Test.Models;

namespace Myth.Repository.Test.Interfaces {

	internal interface IPersonRepository : IReadWriteRepositoryAsync<Person> {
	}
}