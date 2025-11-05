namespace Myth.Vault.Settings {
    public class VaultSettings {
        public ICollection<string> Paths { get; set; } = [];
        public string VaultUrl { get; set; } = null!;
        public string ClusterName { get; set; } = null!;
        public string RoleName { get; set; } = null!;
    }
}
