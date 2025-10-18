namespace Myth.Guard {
	/// <summary>
	/// Represents a validation context key
	/// </summary>
	public readonly record struct  ValidationContextKey( string Key ) {
		public static readonly ValidationContextKey Default = new( "Default" );
		public static readonly ValidationContextKey Create = new( "Create" );
		public static readonly ValidationContextKey Update = new( "Update" );
		public static readonly ValidationContextKey Delete = new( "Delete" );
		public static readonly ValidationContextKey GetByField = new( "GetByField" );
		public static readonly ValidationContextKey GetAll = new( "GetAll" );
		public static readonly ValidationContextKey Search = new( "Search" );
		public static readonly ValidationContextKey Activate = new( "Activate" );
		public static readonly ValidationContextKey Deactivate = new( "Deactivate" );

		public static ValidationContextKey Custom( string key ) => new( key );

		public override string ToString( ) => Key;

		public static implicit operator ValidationContextKey(string key) => new( key );
	}
}