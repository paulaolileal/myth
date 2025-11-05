using Microsoft.Extensions.Configuration;
using Myth.Vault.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Myth.Vault.ValueProviders {
    /// <summary>
    /// Provides secret configuration values from the "Secrets" section of appSettings.json for local environments.
    /// </summary>
    public class LocalSecretConfiguration : ISecretConfiguration {
        private readonly IConfigurationSection _secretSection;

        /// <summary>
        /// Initializes a new instance of the <see cref="LocalSecretConfiguration"/> class using the "Secrets" section from the provided configuration.
        /// </summary>
        /// <param name="configuration">The application configuration instance.</param>
        public LocalSecretConfiguration(IConfiguration configuration) {
            _secretSection = configuration.GetSection("Secrets");
        }

        /// <summary>
        /// Retrieves all keys from the "Secrets" section in appSettings.json.
        /// </summary>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains an enumerable of secret keys.</returns>
        public Task<IEnumerable<string>> GetKeysAsync(CancellationToken cancellationToken) {
            var keys = _secretSection
                .AsEnumerable()
                .Select(x => x.Key.Replace("Secrets:", string.Empty))
                .Where(x => x != "Secrets");

            return Task.FromResult(keys);
        }

        /// <summary>
        /// Retrieves the value of a secret by key from the "Secrets" section in appSettings.json.
        /// </summary>
        /// <typeparam name="T">The type of the value to retrieve.</typeparam>
        /// <param name="key">The key of the secret value.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the secret value, or null if not found.</returns>
        public Task<T?> GetValueAsync<T>(string key, CancellationToken cancellationToken) {
            ArgumentException.ThrowIfNullOrEmpty(key, "The key is required");

            var value = _secretSection.GetValue<T>(key);

            return Task.FromResult(value);
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
        public Task<IDictionary<string, string?>> GetValuesAsync(IEnumerable<string> keys, CancellationToken cancellationToken = default) {
            var result = new Dictionary<string, string?>();

            foreach (var key in keys) {
                var value = _secretSection.GetValue<string>(key);
                result.Add(key, value);
            }
            return Task.FromResult(result as IDictionary<string, string?>);
        }
    }
}
