using System.Security.Cryptography.X509Certificates;

namespace Myth.Models;

/// <summary>
/// Certificate type enumeration for different certificate sources
/// </summary>
public enum CertificateType {

	/// <summary>
	/// PEM certificate with separate key file
	/// </summary>
	PemWithKey,

	/// <summary>
	/// PFX/P12 certificate file
	/// </summary>
	Pfx,

	/// <summary>
	/// Certificate from Windows certificate store
	/// </summary>
	Store,

	/// <summary>
	/// Pre-configured X509Certificate2 instance
	/// </summary>
	X509Certificate2
}

/// <summary>
/// Configuration options for client certificates
/// </summary>
public class CertificateOptions {

	/// <summary>
	/// Type of certificate configuration
	/// </summary>
	public CertificateType Type { get; set; }

	/// <summary>
	/// Path to certificate file (.pem, .pfx, etc.)
	/// </summary>
	public string? CertificatePath { get; set; }

	/// <summary>
	/// Path to private key file (.key)
	/// </summary>
	public string? KeyPath { get; set; }

	/// <summary>
	/// Password for private key or PFX file
	/// </summary>
	public string? Password { get; set; }

	/// <summary>
	/// Certificate data as byte array
	/// </summary>
	public byte[ ]? CertificateData { get; set; }

	/// <summary>
	/// Private key data as byte array
	/// </summary>
	public byte[ ]? KeyData { get; set; }

	/// <summary>
	/// Certificate thumbprint for store lookup
	/// </summary>
	public string? Thumbprint { get; set; }

	/// <summary>
	/// Certificate subject name for store lookup
	/// </summary>
	public string? SubjectName { get; set; }

	/// <summary>
	/// Store location for certificate lookup
	/// </summary>
	public StoreLocation StoreLocation { get; set; } = StoreLocation.CurrentUser;

	/// <summary>
	/// Pre-configured certificate instance
	/// </summary>
	public X509Certificate2? Certificate { get; set; }

	/// <summary>
	/// Whether to validate server certificate
	/// </summary>
	public bool ValidateServerCertificate { get; set; } = true;

	/// <summary>
	/// Additional certificates for certificate chain
	/// </summary>
	public List<X509Certificate2>? AdditionalCertificates { get; set; }

	/// <summary>
	/// Custom server certificate validation callback
	/// </summary>
	public Func<object, X509Certificate?, X509Chain?, System.Net.Security.SslPolicyErrors, bool>? ServerCertificateValidationCallback { get; set; }
}