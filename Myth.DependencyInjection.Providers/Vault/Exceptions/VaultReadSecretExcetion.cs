namespace Myth.Vault.Exceptions {
    public class VaultReadSecretExcetion : Exception {
        public VaultReadSecretExcetion(string? message) : base(message) {
        }

        public VaultReadSecretExcetion(string? message, Exception? innerException) : base(message, innerException) {
        }
    }
}
