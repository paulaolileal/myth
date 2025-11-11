using Microsoft.EntityFrameworkCore;
using Myth.Testing.Test.Models;

namespace Myth.Testing.Test.Repositories;

/// <summary>
/// Example Entity Framework DbContext
/// </summary>
public class UserDbContext : DbContext {

	/// <summary>
	/// Initialize DbContext
	/// </summary>
	/// <param name="options">DbContext options</param>
	public UserDbContext( DbContextOptions<UserDbContext> options ) : base( options ) {
	}

	/// <summary>
	/// Users DbSet
	/// </summary>
	public DbSet<User> Users { get; set; }

	/// <summary>
	/// Configure model
	/// </summary>
	/// <param name="modelBuilder">Model builder</param>
	protected override void OnModelCreating( ModelBuilder modelBuilder ) {
		modelBuilder.Entity<User>( entity => {
			entity.HasKey( e => e.Id );
			entity.Property( e => e.Name ).IsRequired( ).HasMaxLength( 100 );
			entity.Property( e => e.Email ).IsRequired( ).HasMaxLength( 255 );
			entity.HasIndex( e => e.Email ).IsUnique( );
		} );

		base.OnModelCreating( modelBuilder );
	}
}
