using Myth.ValueObjects;

namespace Myth.Morph.Test.Models.Entities;

/// <summary>
/// User role constant for testing Constant&lt;T,V&gt; mapping
/// </summary>
public class UserRole( string name, string value ) : Constant<UserRole, string>( name, value ) {

	public static readonly UserRole Owner = new( nameof( Owner ), "owner" );

	public static readonly UserRole Admin = new( nameof( Admin ), "admin" );

	public static readonly UserRole Editor = new( nameof( Editor ), "editor" );

	public static readonly UserRole Viewer = new( nameof( Viewer ), "viewer" );
}
