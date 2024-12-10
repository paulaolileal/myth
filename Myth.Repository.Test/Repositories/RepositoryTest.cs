using Myth.Repositories.EntityFramework;
using Myth.Repository.Test.Contexts;
using Myth.Repository.Test.Interfaces;
using Myth.Repository.Test.Models;

namespace Myth.Repository.Test.Repositories;

internal class RepositoryTest( ContextTest context ) : ReadWriteRepositoryAsync<Person>( context ), IRepositoryTest {
}