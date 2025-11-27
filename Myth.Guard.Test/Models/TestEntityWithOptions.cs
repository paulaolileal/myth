using Myth.Builder;
using Myth.Extensions;
using Myth.Interfaces;
using Myth.ValueObjects;

namespace Myth.Guard.Test.Models;

/// <summary>
/// Test entity for WithOptions functionality
/// </summary>
public class TestEntityWithOptions : IValidatable<TestEntityWithOptions> {
	public TestStatus Status { get; set; }

	public string Summary { get; set; }

	public void Validate( ValidationBuilder<TestEntityWithOptions> builder, ValidationContextKey? context = null ) {
		builder.For( Status, r => r.IsValidEnumValue( ).WithOptions( Enums.OptionsType.OnlyName ) );
		builder.For( Summary, r => r.NameExistsInConstant<Summary, int>( ).WithOptions( ) );
	}
}

public class Summary : Constant<Summary, int> {
	/// <summary>
	/// Extremely cold conditions, typically below freezing point.
	/// </summary>
	public static readonly Summary Freezing = new( nameof( Freezing ), 0 );

	/// <summary>
	/// Cold and invigorating weather conditions.
	/// </summary>
	public static readonly Summary Bracing = new( nameof( Bracing ), 1 );

	/// <summary>
	/// Noticeably cold but not extremely cold conditions.
	/// </summary>
	public static readonly Summary Chilly = new( nameof( Chilly ), 2 );

	/// <summary>
	/// Pleasantly cold weather conditions.
	/// </summary>
	public static readonly Summary Cool = new( nameof( Cool ), 3 );

	/// <summary>
	/// Moderate and comfortable temperature conditions.
	/// </summary>
	public static readonly Summary Mild = new( nameof( Mild ), 4 );

	/// <summary>
	/// Comfortably hot weather conditions.
	/// </summary>
	public static readonly Summary Warm = new( nameof( Warm ), 5 );

	/// <summary>
	/// Pleasant and soothing warm weather conditions.
	/// </summary>
	public static readonly Summary Balmy = new( nameof( Balmy ), 6 );

	/// <summary>
	/// High temperature conditions.
	/// </summary>
	public static readonly Summary Hot = new( nameof( Hot ), 7 );

	/// <summary>
	/// Extremely hot and humid conditions.
	/// </summary>
	public static readonly Summary Sweltering = new( nameof( Sweltering ), 8 );

	/// <summary>
	/// Intensely hot conditions, typically the highest temperature range.
	/// </summary>
	public static readonly Summary Scorching = new( nameof( Scorching ), 9 );

	private Summary( string name, int value )
		: base( name, value ) {
	}
}
