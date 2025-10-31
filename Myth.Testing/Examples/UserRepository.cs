using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Myth.Testing.Examples {
	/// <summary>
	/// Example repository using Entity Framework
	/// </summary>
	public class UserRepository {
		private readonly UserDbContext _context;

		/// <summary>
		/// Initialize repository with DbContext
		/// </summary>
		/// <param name="context">The database context</param>
		public UserRepository( UserDbContext context ) {
			_context = context ?? throw new ArgumentNullException( nameof( context ) );
		}

		/// <summary>
		/// Create a new user
		/// </summary>
		/// <param name="user">The user to create</param>
		/// <param name="cancellationToken">Cancellation token</param>
		/// <returns>The created user</returns>
		public async Task<UserEntity> CreateAsync( UserEntity user, CancellationToken cancellationToken = default ) {
			if ( user is null )
				throw new ArgumentNullException( nameof( user ) );

			user.Id = Guid.NewGuid( );
			user.CreatedAt = DateTime.UtcNow;

			_context.Users.Add( user );
			await _context.SaveChangesAsync( cancellationToken );

			return user;
		}

		/// <summary>
		/// Get user by ID
		/// </summary>
		/// <param name="id">The user ID</param>
		/// <param name="cancellationToken">Cancellation token</param>
		/// <returns>The user if found, null otherwise</returns>
		public async Task<UserEntity?> GetByIdAsync( Guid id, CancellationToken cancellationToken = default ) {
			return await _context.Users.FindAsync( new object[ ] { id }, cancellationToken );
		}

		/// <summary>
		/// Get user by email
		/// </summary>
		/// <param name="email">The user email</param>
		/// <param name="cancellationToken">Cancellation token</param>
		/// <returns>The user if found, null otherwise</returns>
		public async Task<UserEntity?> GetByEmailAsync( string email, CancellationToken cancellationToken = default ) {
			if ( string.IsNullOrWhiteSpace( email ) )
				return null;

			return await _context.Users
				.FirstOrDefaultAsync( u => u.Email == email, cancellationToken );
		}

		/// <summary>
		/// Get all users
		/// </summary>
		/// <param name="cancellationToken">Cancellation token</param>
		/// <returns>All users</returns>
		public async Task<List<UserEntity>> GetAllAsync( CancellationToken cancellationToken = default ) {
			return await _context.Users.ToListAsync( cancellationToken );
		}

		/// <summary>
		/// Update user
		/// </summary>
		/// <param name="user">The user to update</param>
		/// <param name="cancellationToken">Cancellation token</param>
		/// <returns>The updated user</returns>
		public async Task<UserEntity> UpdateAsync( UserEntity user, CancellationToken cancellationToken = default ) {
			if ( user is null )
				throw new ArgumentNullException( nameof( user ) );

			user.UpdatedAt = DateTime.UtcNow;

			_context.Users.Update( user );
			await _context.SaveChangesAsync( cancellationToken );

			return user;
		}

		/// <summary>
		/// Delete user
		/// </summary>
		/// <param name="id">The user ID</param>
		/// <param name="cancellationToken">Cancellation token</param>
		/// <returns>True if deleted, false if not found</returns>
		public async Task<bool> DeleteAsync( Guid id, CancellationToken cancellationToken = default ) {
			var user = await GetByIdAsync( id, cancellationToken );
			if ( user is null )
				return false;

			_context.Users.Remove( user );
			await _context.SaveChangesAsync( cancellationToken );

			return true;
		}
	}

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
		public DbSet<UserEntity> Users { get; set; }

		/// <summary>
		/// Configure model
		/// </summary>
		/// <param name="modelBuilder">Model builder</param>
		protected override void OnModelCreating( ModelBuilder modelBuilder ) {
			modelBuilder.Entity<UserEntity>( entity => {
				entity.HasKey( e => e.Id );
				entity.Property( e => e.Name ).IsRequired( ).HasMaxLength( 100 );
				entity.Property( e => e.Email ).IsRequired( ).HasMaxLength( 255 );
				entity.HasIndex( e => e.Email ).IsUnique( );
			} );

			base.OnModelCreating( modelBuilder );
		}
	}

	/// <summary>
	/// Example user entity for Entity Framework
	/// </summary>
	public class UserEntity {
		/// <summary>
		/// Gets or sets the user ID
		/// </summary>
		public Guid Id { get; set; }

		/// <summary>
		/// Gets or sets the user name
		/// </summary>
		public string Name { get; set; } = string.Empty;

		/// <summary>
		/// Gets or sets the user email
		/// </summary>
		public string Email { get; set; } = string.Empty;

		/// <summary>
		/// Gets or sets the creation date
		/// </summary>
		public DateTime CreatedAt { get; set; }

		/// <summary>
		/// Gets or sets the last update date
		/// </summary>
		public DateTime? UpdatedAt { get; set; }
	}
}