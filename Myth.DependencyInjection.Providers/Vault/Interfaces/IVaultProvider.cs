using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Myth.Vault.Interfaces {
    public interface IVaultProvider {
        Task<IDictionary<string, string>> GetSecretsAsync(string path, CancellationToken cancellationToken = default);
    }
}
