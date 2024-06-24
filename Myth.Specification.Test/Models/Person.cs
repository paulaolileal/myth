using Bogus;

namespace Myth.Specification.Test.Models {

	internal class Person {
		public Guid PersonId { get; private set; }
		public string Name { get; set; } = null!;
		public string Address { get; set; } = null!;

		protected Person( ) {
		}

		public Person( Guid personId, string name, string address ) {
			PersonId = personId;
			Name = name;
			Address = address;
		}

		public static Person Mock( ) {
			var faker = new Faker( );

			return new Person(
				faker.Random.Guid( ),
				faker.Person.FullName,
				faker.Address.FullAddress( ) );
		}

		public static IEnumerable<Person> Mock( int count ) {
			var result = new List<Person>( );

			for ( var i = 0; i < count; i++ )
				result.Add( Mock( ) );

			return result;
		}
	}
}