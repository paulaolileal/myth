using Bogus;

namespace Myth.Repository.Test.Models;

internal class Person {
	public long PersonId { get; private set; }
	public string Name { get; set; }
	public string Address { get; set; }

	protected Person( ) {
	}

	public Person( string name, string address ) {
		Name = name;
		Address = address;
	}

	public static Person Mock( ) {
		var faker = new Faker( );

		return new Person( faker.Person.FullName, faker.Address.FullAddress( ) );
	}

	public static IEnumerable<Person> Mock( int count ) {
		var result = new List<Person>( );

		for ( int i = 0; i < count; i++ )
			result.Add( Mock( ) );

		return result;
	}
}