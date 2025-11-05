namespace Myth.Vault.Entities {
    public class Secrets {
        public Dictionary<string, string> Data { get; set; } = [];
        public SecretMetadata Metadata { get; set; }
    }
}
