using Microsoft.EntityFrameworkCore;

namespace Myth.Contexts;

/// <summary>
/// A base context for Entity Framework
/// </summary>
/// <param name="options">Options of context</param>
public abstract class BaseContext( DbContextOptions options ) : DbContext( options ) {

	protected override void OnModelCreating( ModelBuilder modelBuilder ) =>
		modelBuilder.ApplyConfigurationsFromAssembly( GetType( ).Assembly );
}