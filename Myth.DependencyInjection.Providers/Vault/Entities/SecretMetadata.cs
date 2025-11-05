namespace Myth.Vault.Entities {
    public class SecretMetadata {
        public string CreatedTime { get; set; }
        public object CustomMetadata { get; set; }
        public string DeletionTime { get; set; }
        public bool Destroyed { get; set; }
        public int Version { get; set; }
    }
}
