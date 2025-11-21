using Microsoft.AspNetCore.Mvc;

namespace Myth.ValueObjects;

/// <summary>
/// Represents pagination information for dividing a collection of items into discrete pages.
/// </summary>
/// <remarks>Use the Default instance for standard pagination (page 1, page size 10). Use the All instance to
/// retrieve all items in a single page, bypassing pagination.</remarks>
/// <param name="pageNumber">The 1-based index of the page to retrieve. Must be greater than 0, or -1 to indicate all items in a single page.</param>
/// <param name="pageSize">The maximum number of items per page. Must be greater than 0, or -1 to indicate all items in a single page.</param>
public class Pagination( int pageNumber, int pageSize ) : ValueObject {

	public Pagination( ) : this( 1, 10 ) {
	}

	/// <summary>
	/// Page number
	/// </summary>
	[FromQuery( Name = "$pagenumber" )]
	public int PageNumber { get; set; } = pageNumber;

	/// <summary>
	/// Page size
	/// </summary>
	[FromQuery( Name = "$pagesize" )]
	public int PageSize { get; set; } = pageSize;

	protected override IEnumerable<object> GetAtomicValues( ) {
		yield return PageNumber;
		yield return PageSize;
	}

	/// <summary>
	/// The default pagination
	/// <para>Page number: 1</para>
	/// <para>Page size: 10</para>
	/// </summary>
	public static readonly Pagination Default = new( );

	/// <summary>
	/// To get all items in only one page
	/// </summary>
	public static readonly Pagination All = new( -1, -1 );
}
