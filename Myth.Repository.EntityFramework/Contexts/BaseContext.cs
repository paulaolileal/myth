using Microsoft.EntityFrameworkCore;

namespace Myth.Contexts {

    public abstract class BaseContext : DbContext {

        public BaseContext( DbContextOptions options, params object[ ] args )
            : base( options ) {
        }

        protected override void OnModelCreating( ModelBuilder modelBuilder ) {
            modelBuilder.ApplyConfigurationsFromAssembly( GetType( ).Assembly );
        }
    }
}