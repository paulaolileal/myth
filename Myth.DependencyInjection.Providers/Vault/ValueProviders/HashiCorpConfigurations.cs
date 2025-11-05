using Microsoft.Extensions.Options;
using Myth.Vault.Interfaces;
using Myth.Vault.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Myth.Vault.ValueProviders {
    public class HashiCorpConfigurations : ISecretConfiguration {
        private readonly IVaultProvider _vaultProvider;
        private readonly ICollection<string> _paths;

        public HashiCorpConfigurations(IOptions<VaultSettings> options) {
            var vaultOptions = options?.Value ?? throw new ArgumentNullException(nameof(options));
            _paths = vaultOptions.Paths;
            _vaultProvider = new VaultProvider(
                vaultOptions.VaultUrl,
                vaultOptions.ClusterName,
                vaultOptions.RoleName);
        }

        /// <summary>
        /// Retrieves all available secrets by merging namespace and application secrets.
        /// </summary>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A dictionary containing all available secrets.</returns>
        private async Task<IDictionary<string, string>> GetAvailableSecretsAsync(CancellationToken cancellationToken = default) {
            var secrets = new List<IDictionary<string, string>>();

            foreach (var path in _paths) {
                var pathSecrets = await _vaultProvider.GetSecretsAsync(path, cancellationToken);
                secrets.Add(pathSecrets);
            }

            return secrets
                .SelectMany(x => x)
                .ToDictionary(
                    x => x.Key.ToLower(),
                    x => x.Value);
        }

        /// <summary>
        /// Gets the value of a secret by its key and converts it to the specified type.
        /// </summary>
        /// <typeparam name="T">The type to convert the secret value to.</typeparam>
        /// <param name="key">The key of the secret.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>The value of the secret converted to the specified type, or default if not found.</returns>
        public async Task<T?> GetValueAsync<T>(string key, CancellationToken cancellationToken = default) {
            ArgumentException.ThrowIfNullOrEmpty(key.ToLower(), "The key is required");

            var secrets = await GetAvailableSecretsAsync(cancellationToken);

            secrets.TryGetValue(key.ToLower(), out var value);

            if (value is null)
                return default;

            return (T)Convert.ChangeType(value, typeof(T));
        }

        /// <summary>
        /// Retrieves the values of the specified secret keys.
        /// </summary>
        /// <typeparam name="T">The type to convert the secret values to (currently not used).</typeparam>
        /// <param name="keys">A collection of secret keys to retrieve values for.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>
        /// A dictionary mapping each requested key to its corresponding secret value, or null if the key is not found.
        /// </returns>
        public async Task<IDictionary<string, string?>> GetValuesAsync(IEnumerable<string> keys, CancellationToken cancellationToken = default) {
            var result = new Dictionary<string, string?>();

            var secrets = await GetAvailableSecretsAsync(cancellationToken);

            foreach (var key in keys) {
                secrets.TryGetValue(key.ToLower(), out var value);
                result.Add(key.ToLower(), value);
            }

            return result;
        }

        /// <summary>
        /// Retrieves all available secret keys.
        /// </summary>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>An enumerable of all secret keys.</returns>
        public async Task<IEnumerable<string>> GetKeysAsync(CancellationToken cancellationToken) {
            var secrets = await GetAvailableSecretsAsync(cancellationToken);

            return secrets.Keys;
        }
    }
}
