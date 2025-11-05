using Microsoft.AspNetCore.Mvc.ModelBinding;
using Myth.Constants;
using Myth.Vault.Entities;
using Myth.Vault.Exceptions;
using Myth.Vault.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Myth.Vault.ValueProviders {
    public class VaultProvider : IVaultProvider {
        public readonly string _baseUrl;
        public readonly string _clusterName;
        public readonly string _roleName;
        public readonly string _namespaceName;

        private string? ClientToken { get; set; }
        private DateTime? CreatedAt { get; set; }
        private AuthMetadata? Metadata { get; set; }

        /// <summary>
        /// Initializes a new instance of <see cref="VaultProvider"/> and loads the required environment settings for communication with Vault.
        /// </summary>
        public VaultProvider(string vaultUrl, string clusterName, string roleName) {
            _baseUrl = vaultUrl;
            _clusterName = clusterName;
            _roleName = roleName;
        }

        /// <summary>
        /// Authenticates with HashiCorp Vault using the Kubernetes authentication method.<br />
        /// Reads the service account JWT and sends it to the Vault endpoint to obtain a client token.<br />
        /// Updates the <see cref="ClientToken"/> and <see cref="CreatedAt"/> properties on success.<br />
        /// Throws <see cref="VaultAuthenticationException"/> in case of authentication failure or file read error.<br />
        /// For more details, see <see href="https://developer.hashicorp.com/vault/api-docs/auth/kubernetes#login">official documentation</see>.<br />
        /// </summary>
        /// <param name="cancellationToken">Cancellation token for the asynchronous operation.</param>
        /// <exception cref="VaultAuthenticationException">Thrown when an authentication or JWT read error occurs.</exception>
        private async Task AuthAsync(CancellationToken cancellationToken = default) {
            const string serviceAccountPath = $"/var/run/secrets/kubernetes.io/serviceaccount/token";
            string jwt = File.ReadAllText(serviceAccountPath);

            try {
                var response = await Rest.Rest
                    .Create()
                    .Configure(conf => conf
                        .WithBaseUrl($"{_baseUrl}/v1/")
                        .WithBodyDeserialization(CaseStrategy.SnakeCase))
                    .DoPost($"auth/{_clusterName}/login", new {
                        role = _roleName,
                        jwt = jwt
                    }).OnResult(res => res
                        .UseTypeForSuccess<HashiCorpAuthResponse>()
                        .UseTypeForNonSuccess<VaultErrorResponse>())
                    .BuildAsync(cancellationToken);

                if (response.IsSuccessStatusCode()) {
                    var result = response.GetAs<HashiCorpAuthResponse>();

                    CreatedAt = DateTime.UtcNow;
                    ClientToken = result.Auth!.ClientToken;
                    Metadata = result.Auth.Metadata;
                }
                else {
                    var error = response.GetAs<VaultErrorResponse>();

                    throw new VaultAuthenticationException(string.Join(", ", error.Errors));
                }
            }
            catch (Exception ex) {
                throw new VaultAuthenticationException(ex.Message);
            }
        }

        /// <summary>
        /// Gets the Vault authentication token, authenticating if necessary.<br />
        /// The token is renewed if it is missing or expired (after 12 hours).<br />
        /// Throws <see cref="VaultAuthenticationException"/> if authentication fails.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token for the asynchronous operation.</param>
        /// <returns>Vault authentication token.</returns>
        /// <exception cref="VaultAuthenticationException">Thrown when an authentication error occurs.</exception>
        public async Task<string> GetTokenAsync(CancellationToken cancellationToken = default) {
            if (string.IsNullOrEmpty(ClientToken) || CreatedAt is null || CreatedAt.Value.AddHours(12) < DateTime.UtcNow)
                await AuthAsync(cancellationToken);

            return ClientToken!;
        }

        /// <summary>
        /// Retrieves the authentication metadata associated with the current Vault session.<br />
        /// If the client token is not available, it will authenticate and obtain a new token.<br />
        /// Returns the metadata information obtained during authentication, or null if unavailable.<br />
        /// </summary>
        /// <param name="cancellationToken">Cancellation token for the asynchronous operation.</param>
        /// <returns>The <see cref="AuthMetadata"/> object containing authentication metadata, or null if not available.</returns>
        public async Task<AuthMetadata?> GetMetadataAsync(CancellationToken cancellationToken = default) {
            if (string.IsNullOrEmpty(ClientToken))
                await GetTokenAsync(cancellationToken);

            return Metadata;
        }

        /// <summary>
        /// Retrieves secrets from HashiCorp Vault at the specified path using the KV engine.<br />
        /// Authenticates if necessary, sends a GET request to the Vault API, and returns the secrets as a dictionary.<br />
        /// Includes the Vault token in the header and expects a snake_case response.<br />
        /// Throws <see cref="VaultReadSecretExcetion"/> in case of failure to read secrets.<br />
        /// For more information, see <see href="https://developer.hashicorp.com/vault/api-docs/secret/kv/kv-v1#read-secret">official documentation</see>.
        /// </summary>
        /// <param name="path">Secret path in Vault.</param>
        /// <param name="cancellationToken">Cancellation token for the asynchronous operation.</param>
        /// <returns>Dictionary containing the retrieved secrets.</returns>
        /// <exception cref="VaultReadSecretExcetion">Thrown when an error occurs while retrieving secrets.</exception>
        public async Task<IDictionary<string, string>> GetSecretsAsync(string path, CancellationToken cancellationToken = default) {
            var token = await GetTokenAsync(cancellationToken);

            var fullPath = $"{_clusterName}/data/{path}";

            try {
                var response = await Rest.Rest
                    .Create()
                    .Configure(conf => conf
                        .WithHeader("X-Vault-Token", token)
                        .WithBaseUrl($"{_baseUrl}/v1/")
                        .WithBodyDeserialization(CaseStrategy.SnakeCase))
                    .DoGet(fullPath)
                    .OnResult(res => res
                        .UseTypeForSuccess<SecretResponse>()
                        .UseTypeForNonSuccess<VaultErrorResponse>())
                    .BuildAsync(cancellationToken);

                if (response.IsSuccessStatusCode()) {
                    var secrets = response.GetAs<SecretResponse>();
                    var result = secrets.Data.Data.ToDictionary(x => x.Key, x => x.Value);
                    return result;
                }
                else {
                    var error = response.GetAs<VaultErrorResponse>();
                    throw new VaultReadSecretExcetion($"Failed to retrieve secrets from path '{fullPath}': {string.Join(", ", error.Errors)}");
                }
            }
            catch (Exception ex) {
                throw new VaultReadSecretExcetion($"Failed to retrieve secrets from path '{fullPath}': {ex.Message}", ex);
            }
        }



    }
}