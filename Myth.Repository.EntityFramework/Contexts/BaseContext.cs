using Microsoft.EntityFrameworkCore;

namespace Myth.Contexts {

    public abstract class BaseContext : DbContext {

        public BaseContext( DbContextOptions options )
            : base( options ) {
        }

        protected override void OnModelCreating( ModelBuilder modelBuilder ) {
            modelBuilder.ApplyConfigurationsFromAssembly( GetType( ).Assembly );
        }
    }
}