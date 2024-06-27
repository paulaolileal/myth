using Microsoft.EntityFrameworkCore;

namespace Myth.Contexts;

public abstract class BaseContext( DbContextOptions options ) : DbContext( options ) {

	protected override void OnModelCreating( ModelBuilder modelBuilder ) =>
		modelBuilder.ApplyConfigurationsFromAssembly( GetType( ).Assembly );
}