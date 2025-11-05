using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Myth.Vault.Exceptions {
    public class VaultAuthenticationException : Exception {
        public VaultAuthenticationException(string? message)
            : base($"Error on executing authorization on Vault!\nMessage: {message}") {
        }
    }
}
