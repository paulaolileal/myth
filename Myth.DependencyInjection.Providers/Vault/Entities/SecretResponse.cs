namespace Myth.Vault.Entities {
    public class SecretResponse {
        public string RequestId { get; set; } = null!;
        public string LeaseId { get; set; } = null!;
        public bool Renewable { get; set; }
        public int LeaseDuration { get; set; }
        public Secrets? Data { get; set; }
        public object? WrapInfo { get; set; }
        public object? Warnings { get; set; }
        public object? Auth { get; set; }
        public string MountType { get; set; } = null!;
    }
}
