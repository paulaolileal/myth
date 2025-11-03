using FluentAssertions;
using Myth.Guard.Test.Models;
using Myth.Validation;

namespace Myth.Guard.Test;

/// <summary>
/// Tests for collection validation rules
/// </summary>
public class CollectionRulesTests : BaseTestFixture {
	private readonly Validator _validator;

	public CollectionRulesTests( ) {
		_validator = new Validator( ServiceProvider );
	}

	[Fact]
	public async Task NotEmpty_WithValidCollections_ShouldPass( ) {
		// Arrange
		var entity = new CollectionTestEntity {
			Items = new List<string> { "item1", "item2" },
			Tags = new[ ] { "tag1", "tag2", "tag3" },
			Numbers = new[ ] { 1, 2, 3 }
		};

		// Act
		var result = await _validator.ValidateAndReturnAsync( entity );

		// Assert
		result.IsValid.Should( ).BeTrue( );
	}

	[Fact]
	public async Task NotEmpty_WithEmptyList_ShouldFail( ) {
		// Arrange
		var entity = new CollectionTestEntity {
			Items = new List<string>( ),
			Tags = new[ ] { "tag1" },
			Numbers = new[ ] { 1 }
		};

		// Act
		var result = await _validator.ValidateAndReturnAsync( entity );

		// Assert
		result.IsValid.Should( ).BeFalse( );
		result.Errors.Should( ).Contain( e => e.Field == "Items" );
	}

	[Fact]
	public async Task CountBetween_WithValidCount_ShouldPass( ) {
		// Arrange
		var entity = new CollectionTestEntity {
			Items = new List<string> { "item1", "item2", "item3" }, // Count 3, valid (1-5)
			Tags = new[ ] { "tag1" }, // Count 1, valid (> 0)
			Numbers = new[ ] { 1 } // Count 1, valid (has positive, no negative)
		};

		// Act
		var result = await _validator.ValidateAndReturnAsync( entity );

		// Assert
		result.IsValid.Should( ).BeTrue( );
	}

	[Fact]
	public async Task CountBetween_WithTooManyItems_ShouldFail( ) {
		// Arrange
		var entity = new CollectionTestEntity {
			Items = new List<string> { "item1", "item2", "item3", "item4", "item5", "item6" }, // Count 6, exceeds max (5)
			Tags = new[ ] { "tag1" },
			Numbers = new[ ] { 1 }
		};

		// Act
		var result = await _validator.ValidateAndReturnAsync( entity );

		// Assert
		result.IsValid.Should( ).BeFalse( );
		result.Errors.Should( ).Contain( e => e.Field == "Items" );
	}

	[Fact]
	public async Task All_WithValidItems_ShouldPass( ) {
		// Arrange
		var entity = new CollectionTestEntity {
			Items = new List<string> { "item1", "item2", "item3" },
			Tags = new[ ] { "tag1" },
			Numbers = new[ ] { 1 }
		};

		// Act
		var result = await _validator.ValidateAndReturnAsync( entity );

		// Assert
		result.IsValid.Should( ).BeTrue( );
	}

	[Fact]
	public async Task All_WithInvalidItems_ShouldFail( ) {
		// Test with empty string
		var entity1 = new CollectionTestEntity {
			Items = new List<string> { "item1", "", "item3" },
			Tags = new[ ] { "tag1" },
			Numbers = new[ ] { 1 }
		};

		var result1 = await _validator.ValidateAndReturnAsync( entity1 );
		result1.IsValid.Should( ).BeFalse( );
		result1.Errors.Should( ).Contain( e => e.Field == "Items" );

		// Test with whitespace only
		var entity2 = new CollectionTestEntity {
			Items = new List<string> { "item1", "   ", "item3" },
			Tags = new[ ] { "tag1" },
			Numbers = new[ ] { 1 }
		};

		var result2 = await _validator.ValidateAndReturnAsync( entity2 );
		result2.IsValid.Should( ).BeFalse( );
		result2.Errors.Should( ).Contain( e => e.Field == "Items" );

		// Test with null
		var entity3 = new CollectionTestEntity {
			Items = new List<string> { "item1", null!, "item3" },
			Tags = new[ ] { "tag1" },
			Numbers = new[ ] { 1 }
		};

		var result3 = await _validator.ValidateAndReturnAsync( entity3 );
		result3.IsValid.Should( ).BeFalse( );
		result3.Errors.Should( ).Contain( e => e.Field == "Items" );
	}

	[Fact]
	public async Task CountGreaterThan_WithValidCount_ShouldPass( ) {
		// Arrange
		var entity = new CollectionTestEntity {
			Items = new List<string> { "item1" },
			Tags = new[ ] { "tag1", "tag2" },
			Numbers = new[ ] { 1 }
		};

		// Act
		var result = await _validator.ValidateAndReturnAsync( entity );

		// Assert
		result.IsValid.Should( ).BeTrue( );
	}

	[Fact]
	public async Task CountGreaterThan_WithZeroCount_ShouldFail( ) {
		// Arrange
		var entity = new CollectionTestEntity {
			Items = new List<string> { "item1" },
			Tags = Array.Empty<string>( ),
			Numbers = new[ ] { 1 }
		};

		// Act
		var result = await _validator.ValidateAndReturnAsync( entity );

		// Assert
		result.IsValid.Should( ).BeFalse( );
		result.Errors.Should( ).Contain( e => e.Field == "Tags" );
	}

	[Fact]
	public async Task Distinct_WithUniqueItems_ShouldPass( ) {
		// Arrange
		var entity = new CollectionTestEntity {
			Items = new List<string> { "item1" },
			Tags = new[ ] { "tag1", "tag2", "tag3" },
			Numbers = new[ ] { 1 }
		};

		// Act
		var result = await _validator.ValidateAndReturnAsync( entity );

		// Assert
		result.IsValid.Should( ).BeTrue( );
	}

	[Fact]
	public async Task Distinct_WithDuplicateItems_ShouldFail( ) {
		// Arrange
		var entity = new CollectionTestEntity {
			Items = new List<string> { "item1" },
			Tags = new[ ] { "tag1", "tag2", "tag1" },
			Numbers = new[ ] { 1 }
		};

		// Act
		var result = await _validator.ValidateAndReturnAsync( entity );

		// Assert
		result.IsValid.Should( ).BeFalse( );
		result.Errors.Should( ).Contain( e => e.Field == "Tags" );
	}

	[Fact]
	public async Task Any_WithConditionMet_ShouldPass( ) {
		// Arrange
		var entity = new CollectionTestEntity {
			Items = new List<string> { "item1" },
			Tags = new[ ] { "tag1" },
			Numbers = new[ ] { 1, 0, 5 }
		};

		// Act
		var result = await _validator.ValidateAndReturnAsync( entity );

		// Assert
		result.IsValid.Should( ).BeTrue( );
	}

	[Fact]
	public async Task Any_WithConditionNotMet_ShouldFail( ) {
		// Arrange
		var entity = new CollectionTestEntity {
			Items = new List<string> { "item1" },
			Tags = new[ ] { "tag1" },
			Numbers = new[ ] { -1, -2, 0 }
		};

		// Act
		var result = await _validator.ValidateAndReturnAsync( entity );

		// Assert
		result.IsValid.Should( ).BeFalse( );
		result.Errors.Should( ).Contain( e => e.Field == "Numbers" );
	}

	[Fact]
	public async Task None_WithConditionNotMet_ShouldPass( ) {
		// Arrange
		var entity = new CollectionTestEntity {
			Items = new List<string> { "item1" },
			Tags = new[ ] { "tag1" },
			Numbers = new[ ] { 0, 1, 2 }
		};

		// Act
		var result = await _validator.ValidateAndReturnAsync( entity );

		// Assert
		result.IsValid.Should( ).BeTrue( );
	}

	[Fact]
	public async Task None_WithConditionMet_ShouldFail( ) {
		// Arrange
		var entity = new CollectionTestEntity {
			Items = new List<string> { "item1" },
			Tags = new[ ] { "tag1" },
			Numbers = new[ ] { 1, 2, -1 }
		};

		// Act
		var result = await _validator.ValidateAndReturnAsync( entity );

		// Assert
		result.IsValid.Should( ).BeFalse( );
		result.Errors.Should( ).Contain( e => e.Field == "Numbers" );
	}

	[Fact]
	public async Task TestUser_Tags_WithValidTags_ShouldPass( ) {
		// Arrange
		var user = new TestUser {
			Name = "TestUser",
			Email = "test@example.com",
			Age = 25,
			BirthDate = DateTime.Now.AddYears( -25 ),
			RegistrationDate = DateOnly.FromDateTime( DateTime.Today ),
			Tags = new List<string> { "developer", "remote", "senior" },
			IsActive = true,
			Role = UserRole.User,
			Salary = 50000,
			Score = 85.5
		};

		// Act
		var result = await _validator.ValidateAndReturnAsync( user );

		// Assert
		result.IsValid.Should( ).BeTrue( );
	}

	[Fact]
	public async Task TestUser_Tags_WithEmptyTags_ShouldFail( ) {
		// Arrange
		var user = new TestUser {
			Name = "TestUser",
			Email = "test@example.com",
			Age = 25,
			BirthDate = DateTime.Now.AddYears( -25 ),
			RegistrationDate = DateOnly.FromDateTime( DateTime.Today ),
			Tags = new List<string>( ),
			IsActive = true,
			Role = UserRole.User,
			Salary = 50000,
			Score = 85.5
		};

		// Act
		var result = await _validator.ValidateAndReturnAsync( user );

		// Assert
		result.IsValid.Should( ).BeFalse( );
		result.Errors.Should( ).Contain( e => e.Field == "Tags" );
	}

	[Fact]
	public async Task TestUser_Tags_WithTooManyTags_ShouldFail( ) {
		// Arrange
		var tooManyTags = Enumerable.Range( 1, 11 ).Select( i => $"tag{i}" ).ToList( );

		var user = new TestUser {
			Name = "TestUser",
			Email = "test@example.com",
			Age = 25,
			BirthDate = DateTime.Now.AddYears( -25 ),
			RegistrationDate = DateOnly.FromDateTime( DateTime.Today ),
			Tags = tooManyTags,
			IsActive = true,
			Role = UserRole.User,
			Salary = 50000,
			Score = 85.5
		};

		// Act
		var result = await _validator.ValidateAndReturnAsync( user );

		// Assert
		result.IsValid.Should( ).BeFalse( );
		result.Errors.Should( ).Contain( e => e.Field == "Tags" );
	}

	[Fact]
	public async Task TestUser_Tags_WithInvalidTags_ShouldFail( ) {
		// Arrange
		var user = new TestUser {
			Name = "TestUser",
			Email = "test@example.com",
			Age = 25,
			BirthDate = DateTime.Now.AddYears( -25 ),
			RegistrationDate = DateOnly.FromDateTime( DateTime.Today ),
			Tags = new List<string> { "valid", "", "   " },
			IsActive = true,
			Role = UserRole.User,
			Salary = 50000,
			Score = 85.5
		};

		// Act
		var result = await _validator.ValidateAndReturnAsync( user );

		// Assert
		result.IsValid.Should( ).BeFalse( );
		result.Errors.Should( ).Contain( e => e.Field == "Tags" );
	}
}