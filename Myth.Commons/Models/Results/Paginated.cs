using Myth.Interfaces.Results;

namespace Myth.Models.Results;

public class Paginated<TEntity>(
	int pageNumber,
	int pageSize,
	int totalItems,
	int totalPages,
	IEnumerable<TEntity> items ) : IPaginated<TEntity> {
	public int PageNumber { get; private set; } = pageNumber;

	public int PageSize { get; private set; } = pageSize;

	public int TotalItems { get; private set; } = totalItems;

	public int TotalPages { get; private set; } = totalPages;

	public IEnumerable<TEntity> Items { get; private set; } = items;
}