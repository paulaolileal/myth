using Myth.Repositories.EntityFramework;
using Myth.Repository.Test.Contexts;
using Myth.Repository.Test.Interfaces;
using Myth.Repository.Test.Models;

namespace Myth.Repository.Test.Repositories;

/// <summary>
/// Non-test repository implementation for testing AddRepositories functionality
/// </summary>
internal class PersonRepository( ContextTest context ) : ReadWriteRepositoryAsync<Person>( context ), IPersonRepository {
}