using Microsoft.AspNetCore.Mvc;

namespace Myth.ValueObjects;

public class Pagination( int pageNumber, int pageSize ) : ValueObject {

	public Pagination( ) : this( 1, 10 ) { }

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