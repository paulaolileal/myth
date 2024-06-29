namespace Myth.Interfaces.Repositories.Results;

public interface IPaginated<T> : IPaginated {
	/// <summary>
	/// The items of page
	/// </summary>
	public IEnumerable<T> Items { get; }
}