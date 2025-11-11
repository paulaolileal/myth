namespace Myth.Morph.Test.Service;

internal interface IDescriptionResolver {

	string Resove( string text );
}

internal class DescriptionResolver : IDescriptionResolver {

	public string Resove( string text ) => text.ToUpper( );
}
