namespace Myth.Interfaces.Repositories.Results;

public interface IPaginated {
	/// <summary>
	/// The number of page
	/// </summary>
	public int PageNumber { get; }

	/// <summary>
	/// The size of page
	/// </summary>
	public int PageSize { get; }

	/// <summary>
	/// The amount of pages
	/// </summary>
	public int TotalPages { get; }

	/// <summary>
	/// The total of items between all pages
	/// </summary>
	public int TotalItems { get; }
}