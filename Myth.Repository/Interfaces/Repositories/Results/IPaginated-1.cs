namespace Myth.Interfaces.Repositories.Results {

	public interface IPaginated<T> : IPaginated {
		public IEnumerable<T> Items { get; }
	}
}