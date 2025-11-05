namespace Myth.Vault.Interfaces {
    public interface ISecretConfiguration {
        Task<IEnumerable<string>> GetKeysAsync(CancellationToken cancellationToken);

        Task<T?> GetValueAsync<T>(string key, CancellationToken cancellationToken);

        Task<IDictionary<string, string?>> GetValuesAsync(IEnumerable<string> keys, CancellationToken cancellationToken = default);
    }
}
