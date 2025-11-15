namespace Myth.Interfaces.Results;

public interface IPaginated<T> : IPaginated {

	/// <summary>
	/// The items of page
	/// </summary>
	public IEnumerable<T> Items { get; }
}
