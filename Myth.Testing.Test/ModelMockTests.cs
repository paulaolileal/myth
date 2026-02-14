using Bogus;
using FluentAssertions;
using Myth.Testing.Test.Models;

namespace Myth.Testing.Test.Mocks;

/// <summary>
/// Tests for ModelMock base class using UserMock implementation
/// </summary>
public class ModelMockTests {
	private readonly Faker _faker;
	private readonly UserMock _userMock;

	/// <summary>
	/// Initialize tests with Faker and UserMock
	/// </summary>
	public ModelMockTests( ) {
		_faker = new Faker( "en_US" );
		_userMock = new UserMock( _faker );
	}

	/// <summary>
	/// Test generating a single user
	/// </summary>
	[Fact]
	public void Generate_WithoutAmount_ShouldReturnSingleUser( ) {
		// Act
		var user = _userMock.Generate( );

		// Assert
		user.Should( ).NotBeNull( );
		user.Should( ).BeOfType<User>( );
		user.Id.Should( ).NotBeEmpty( );
		user.Name.Should( ).NotBeNullOrEmpty( );
		user.Email.Should( ).NotBeNullOrEmpty( );
		user.CreatedAt.Should( ).BeBefore( DateTime.UtcNow );
	}

	/// <summary>
	/// Test generating multiple users
	/// </summary>
	[Fact]
	public void Generate_WithAmount_ShouldReturnSpecifiedNumberOfUsers( ) {
		// Arrange
		int expectedCount = 5;

		// Act
		var users = _userMock.Generate( expectedCount );

		// Assert
		users.Should( ).NotBeNull( );
		users.Should( ).HaveCount( expectedCount );
		users.Should( ).AllBeOfType<User>( );
		users.Select( u => u.Id ).Should( ).OnlyHaveUniqueItems( );
	}

	/// <summary>
	/// Test generating users with metadata
	/// </summary>
	[Fact]
	public void Generate_WithMetadata_ShouldApplyMetadataValues( ) {
		// Arrange
		var metadata = new Dictionary<string, object> {
			{ "Name", "John Doe" },
			{ "Email", "john.doe@example.com" }
		};

		// Act
		var users = _userMock.Generate( 1, metadata );
		var user = users.First( );

		// Assert
		user.Should( ).NotBeNull( );
		user.Name.Should( ).Be( "John Doe" );
		user.Email.Should( ).Be( "john.doe@example.com" );
	}

	/// <summary>
	/// Test generating multiple users with metadata
	/// </summary>
	[Fact]
	public void Generate_WithAmountAndMetadata_ShouldApplyMetadataToAll( ) {
		// Arrange
		var metadata = new Dictionary<string, object> {
			{ "Name", "Test User" }
		};

		// Act
		var users = _userMock.Generate( 3, metadata );

		// Assert
		users.Should( ).HaveCount( 3 );
		users.Should( ).AllSatisfy( u => u.Name.Should( ).Be( "Test User" ) );
	}

	/// <summary>
	/// Test generating users without metadata
	/// </summary>
	[Fact]
	public void Generate_WithNullMetadata_ShouldGenerateRandomData( ) {
		// Act
		var users = _userMock.Generate( 3, null );

		// Assert
		users.Should( ).HaveCount( 3 );
		users.Select( u => u.Name ).Should( ).OnlyHaveUniqueItems( );
		users.Select( u => u.Email ).Should( ).OnlyHaveUniqueItems( );
	}

	/// <summary>
	/// Test that generated users have valid email format
	/// </summary>
	[Fact]
	public void Generate_ShouldCreateUsersWithValidEmailFormat( ) {
		// Act
		var users = _userMock.Generate( 10 );

		// Assert
		users.Should( ).AllSatisfy( u => {
			u.Email.Should( ).Contain( "@" );
			u.Email.Should( ).Contain( "." );
		} );
	}

	/// <summary>
	/// Test that generated users have unique IDs
	/// </summary>
	[Fact]
	public void Generate_ShouldCreateUsersWithUniqueIds( ) {
		// Act
		var users = _userMock.Generate( 20 );

		// Assert
		var ids = users.Select( u => u.Id ).ToList( );
		ids.Should( ).OnlyHaveUniqueItems( );
		ids.Should( ).AllSatisfy( id => id.Should( ).NotBeEmpty( ) );
	}

	/// <summary>
	/// Test that CreatedAt is in the past
	/// </summary>
	[Fact]
	public void Generate_ShouldSetCreatedAtInThePast( ) {
		// Act
		var users = _userMock.Generate( 5 );

		// Assert
		users.Should( ).AllSatisfy( u => 
			u.CreatedAt.Should( ).BeBefore( DateTime.UtcNow ) 
		);
	}

	/// <summary>
	/// Test that UpdatedAt is recent
	/// </summary>
	[Fact]
	public void Generate_ShouldSetUpdatedAtRecently( ) {
		// Act
		var users = _userMock.Generate( 5 );

		// Assert
		users.Should( ).AllSatisfy( u => {
			if ( u.UpdatedAt.HasValue ) {
				u.UpdatedAt.Value.Should( ).BeCloseTo( DateTime.UtcNow, TimeSpan.FromDays( 7 ) );
			}
		} );
	}

	/// <summary>
	/// Test generating large number of users
	/// </summary>
	[Fact]
	public void Generate_WithLargeAmount_ShouldHandleEfficiently( ) {
		// Act
		var users = _userMock.Generate( 1000 );

		// Assert
		users.Should( ).HaveCount( 1000 );
		users.Select( u => u.Id ).Should( ).OnlyHaveUniqueItems( );
	}

	/// <summary>
	/// Test partial metadata application
	/// </summary>
	[Fact]
	public void Generate_WithPartialMetadata_ShouldOnlyOverrideSpecifiedFields( ) {
		// Arrange
		var metadata = new Dictionary<string, object> {
			{ "Email", "fixed@example.com" }
		};

		// Act
		var users = _userMock.Generate( 3, metadata );

		// Assert
		users.Should( ).AllSatisfy( u => u.Email.Should( ).Be( "fixed@example.com" ) );
		users.Select( u => u.Name ).Should( ).OnlyHaveUniqueItems( );
		users.Select( u => u.Id ).Should( ).OnlyHaveUniqueItems( );
	}

	/// <summary>
	/// Test that Faker culture is respected
	/// </summary>
	[Fact]
	public void Generate_ShouldUseFakerCulture( ) {
		// Arrange
		var germanFaker = new Faker( "de" );
		var germanUserMock = new UserMock( germanFaker );

		// Act
		var users = germanUserMock.Generate( 10 );

		// Assert
		users.Should( ).HaveCount( 10 );
		users.Should( ).AllSatisfy( u => {
			u.Name.Should( ).NotBeNullOrEmpty( );
			u.Email.Should( ).NotBeNullOrEmpty( );
		} );
	}

	/// <summary>
	/// Test generating zero users
	/// </summary>
	[Fact]
	public void Generate_WithZeroAmount_ShouldReturnEmptyCollection( ) {
		// Act
		var users = _userMock.Generate( 0 );

		// Assert
		users.Should( ).NotBeNull( );
		users.Should( ).BeEmpty( );
	}

	/// <summary>
	/// Test that all required fields are populated
	/// </summary>
	[Fact]
	public void Generate_ShouldPopulateAllRequiredFields( ) {
		// Act
		var user = _userMock.Generate( );

		// Assert
		user.Id.Should( ).NotBeEmpty( );
		user.Name.Should( ).NotBeNullOrEmpty( );
		user.Email.Should( ).NotBeNullOrEmpty( );
		user.CreatedAt.Should( ).NotBe( default );
	}
}
