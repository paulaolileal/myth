namespace Myth.Extensions {

	public static class EnumerableExtension {

		public static string ToStringWithSeparator( this IEnumerable<string> list, string separator = ", " ) {
			if ( list != null && list.Any() )
				return string.Join( separator, list.ToArray( ) );

			return string.Empty;
		}
	}
}