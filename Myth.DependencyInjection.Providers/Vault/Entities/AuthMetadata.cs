namespace Myth.Vault.Entities {
    public class AuthMetadata {
        public string Role { get; set; } = null!;
        public string ServiceAccountName { get; set; } = null!;
        public string ServiceAccountNamespace { get; set; } = null!;
        public string ServiceAccountSecretName { get; set; } = null!;
        public string ServiceAccountUid { get; set; } = null!;
    }
}
