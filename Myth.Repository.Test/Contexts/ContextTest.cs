using Microsoft.EntityFrameworkCore;
using Myth.Contexts;
using Myth.Repository.Test.Models;

namespace Myth.Repository.Test.Contexts;

internal class ContextTest( DbContextOptions options ) : BaseContext( options ) {
	public DbSet<Person> Persons { get; set; }

	protected override void OnModelCreating( ModelBuilder modelBuilder ) {
		base.OnModelCreating( modelBuilder );

		var entity = modelBuilder.Entity<Person>( );
		entity.ToTable( "persons" );

		entity
			.Property( x => x.PersonId )
			.HasColumnName( "person_id" );

		entity.Property( x => x.PersonId ).ValueGeneratedOnAdd( );

		entity
			.Property( x => x.Name )
			.HasColumnName( "name" );

		entity
			.Property( x => x.Address )
			.HasColumnName( "address" );
	}
}