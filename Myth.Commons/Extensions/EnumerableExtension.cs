namespace Myth.Extensions;

public static class EnumerableExtension {

	/// <summary>
	/// Join an array of strings into a single string using a character as separator
	/// </summary>
	/// <param name="list">A list of strings</param>
	/// <param name="separator">A character to be used between strings</param>
	/// <returns>A single string separated by a character</returns>
	/// <remarks>
	/// <para>
	/// Give the following array of strings: <c>["a", "b", "c"]</c>
	/// </para>
	/// <para>
	/// Using the separator: <c>", </c>
	/// </para>
	/// Produces: "a, b, c"
	/// </remarks>
	public static string ToStringWithSeparator( this IEnumerable<string> list, string separator = ", " ) {
		if ( list != null && list.Any( ) )
			return string
				.Join( separator, list.ToArray( ) )
				.TrimEnd( );

		return string.Empty;
	}
}
