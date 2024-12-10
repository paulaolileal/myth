using Bogus;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Myth.Interfaces.Repositories.EntityFramework;
using Myth.Repositories.EntityFramework;
using Myth.Repository.Test.Contexts;
using Myth.Repository.Test.Interfaces;
using Myth.Repository.Test.Repositories;
using Person = Myth.Repository.Test.Models.Person;

namespace Myth.Repository.Test.Mocks;

public class DatabaseMock : IDisposable {
	internal ContextTest Context { get; }
	internal IRepositoryTest Repository { get; }
	internal IUnitOfWorkRepository UnitOfWork { get; }
	internal Faker Faker { get; }

	public DatabaseMock( ) {
		Faker = new Faker( );

		var dbContextOptionsBuilder = new DbContextOptionsBuilder( )
			.UseInMemoryDatabase( "DataBaseTest" )
			.ConfigureWarnings( warnings => warnings
				.Ignore( InMemoryEventId.TransactionIgnoredWarning ) );

		Context = new ContextTest( dbContextOptionsBuilder.Options );

		Repository = new RepositoryTest( Context );

		UnitOfWork = new UnitOfWorkRepository( Context );
	}

	internal async Task<Person> MockAsync( ) {
		var person = Person.Mock( );

		await Context.AddAsync( person );
		Context.SaveChanges( );

		return person;
	}

	internal async Task<IEnumerable<Person>> MockAsync( int count ) {
		var persons = Person.Mock( count );

		await Context.AddRangeAsync( persons );
		Context.SaveChanges( );

		return persons;
	}

	public void Dispose( ) {
		Context.Database.EnsureDeleted( );

		Context.Dispose( );

		UnitOfWork.DisposeAsync( ).GetAwaiter( ).GetResult( );

		Repository.DisposeAsync( ).GetAwaiter( ).GetResult( );
	}
}