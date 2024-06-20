using Myth.Interfaces.Repositories.EntityFramework;
using Myth.Repository.Test.Models;

namespace Myth.Repository.Test.Interfaces {

	internal interface IRepositoryTest : IReadWriteRepositoryAsync<Person> {
	}
}