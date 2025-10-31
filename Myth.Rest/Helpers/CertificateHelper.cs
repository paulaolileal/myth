using Myth.Models;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Myth.Helpers;

/// <summary>
/// Helper class for certificate operations
/// </summary>
public static class CertificateHelper {

	/// <summary>
	/// Load certificate based on configuration options
	/// </summary>
	/// <param name="options">Certificate configuration options</param>
	/// <returns>Loaded X509Certificate2</returns>
	/// <exception cref="ArgumentException">Thrown when configuration is invalid</exception>
	/// <exception cref="FileNotFoundException">Thrown when certificate file is not found</exception>
	/// <exception cref="InvalidOperationException">Thrown when certificate cannot be loaded</exception>
	public static X509Certificate2 LoadCertificate( CertificateOptions options ) {
		ArgumentNullException.ThrowIfNull( options, nameof( options ) );

		return options.Type switch {
			CertificateType.PemWithKey => LoadPemCertificate( options ),
			CertificateType.Pfx => LoadPfxCertificate( options ),
			CertificateType.Store => LoadStoreCertificate( options ),
			CertificateType.X509Certificate2 => options.Certificate ?? throw new ArgumentException( "Certificate instance is required for X509Certificate2 type", nameof( options ) ),
			_ => throw new ArgumentException( $"Unsupported certificate type: {options.Type}", nameof( options ) )
		};
	}

	/// <summary>
	/// Configure HttpClientHandler with client certificate
	/// </summary>
	/// <param name="handler">HttpClientHandler to configure</param>
	/// <param name="options">Certificate configuration options</param>
	public static void ConfigureHandler( HttpClientHandler handler, CertificateOptions options ) {
		ArgumentNullException.ThrowIfNull( handler, nameof( handler ) );
		ArgumentNullException.ThrowIfNull( options, nameof( options ) );

		var certificate = LoadCertificate( options );
		handler.ClientCertificates.Add( certificate );

		// Add additional certificates if provided
		if ( options.AdditionalCertificates?.Any( ) == true ) {
			foreach ( var additionalCert in options.AdditionalCertificates ) {
				handler.ClientCertificates.Add( additionalCert );
			}
		}

		// Configure server certificate validation
		if ( !options.ValidateServerCertificate || options.ServerCertificateValidationCallback != null ) {
			handler.ServerCertificateCustomValidationCallback = options.ServerCertificateValidationCallback
				?? ( ( sender, cert, chain, sslPolicyErrors ) => !options.ValidateServerCertificate );
		}
	}

	/// <summary>
	/// Load PEM certificate with private key
	/// </summary>
	/// <param name="options">Certificate options</param>
	/// <returns>X509Certificate2 with private key</returns>
	private static X509Certificate2 LoadPemCertificate( CertificateOptions options ) {
		string certPem, keyPem;

		if ( !string.IsNullOrEmpty( options.CertificatePath ) && !string.IsNullOrEmpty( options.KeyPath ) ) {
			if ( !File.Exists( options.CertificatePath ) )
				throw new FileNotFoundException( $"Certificate file not found: {options.CertificatePath}" );

			if ( !File.Exists( options.KeyPath ) )
				throw new FileNotFoundException( $"Private key file not found: {options.KeyPath}" );

			certPem = File.ReadAllText( options.CertificatePath );
			keyPem = File.ReadAllText( options.KeyPath );
		} else if ( options.CertificateData != null && options.KeyData != null ) {
			certPem = Encoding.UTF8.GetString( options.CertificateData );
			keyPem = Encoding.UTF8.GetString( options.KeyData );
		} else {
			throw new ArgumentException( "Either file paths or data must be provided for PEM certificate", nameof( options ) );
		}

		try {
			// Create certificate from PEM data
			var certificate = X509Certificate2.CreateFromPem( certPem, keyPem );

			// If password is provided, we might need to handle encrypted private keys
			if ( !string.IsNullOrEmpty( options.Password ) ) {
				// For encrypted private keys, we need to use a different approach
				certificate = LoadEncryptedPemCertificate( certPem, keyPem, options.Password );
			}

			return certificate;
		} catch ( Exception ex ) {
			throw new InvalidOperationException( "Failed to load PEM certificate", ex );
		}
	}

	/// <summary>
	/// Load encrypted PEM certificate
	/// </summary>
	/// <param name="certPem">Certificate PEM content</param>
	/// <param name="keyPem">Private key PEM content</param>
	/// <param name="password">Private key password</param>
	/// <returns>X509Certificate2 with private key</returns>
	private static X509Certificate2 LoadEncryptedPemCertificate( string certPem, string keyPem, string password ) {
		try {
			// Try to load with password
			var cert = X509Certificate2.CreateFromEncryptedPem( certPem, keyPem, password );
			return cert;
		} catch {
			// Fallback: try to load as unencrypted
			return X509Certificate2.CreateFromPem( certPem, keyPem );
		}
	}

	/// <summary>
	/// Load PFX certificate
	/// </summary>
	/// <param name="options">Certificate options</param>
	/// <returns>X509Certificate2</returns>
	private static X509Certificate2 LoadPfxCertificate( CertificateOptions options ) {
		try {
			if ( !string.IsNullOrEmpty( options.CertificatePath ) ) {
				if ( !File.Exists( options.CertificatePath ) )
					throw new FileNotFoundException( $"PFX file not found: {options.CertificatePath}" );

				return new X509Certificate2( options.CertificatePath, options.Password, X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet );
			} else if ( options.CertificateData != null ) {
				return new X509Certificate2( options.CertificateData, options.Password, X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet );
			} else {
				throw new ArgumentException( "Either file path or data must be provided for PFX certificate", nameof( options ) );
			}
		} catch ( Exception ex ) when ( !( ex is FileNotFoundException || ex is ArgumentException ) ) {
			throw new InvalidOperationException( "Failed to load PFX certificate", ex );
		}
	}

	/// <summary>
	/// Load certificate from Windows certificate store
	/// </summary>
	/// <param name="options">Certificate options</param>
	/// <returns>X509Certificate2</returns>
	private static X509Certificate2 LoadStoreCertificate( CertificateOptions options ) {
		using var store = new X509Store( StoreName.My, options.StoreLocation );
		store.Open( OpenFlags.ReadOnly );

		X509Certificate2Collection certificates;

		if ( !string.IsNullOrEmpty( options.Thumbprint ) ) {
			certificates = store.Certificates.Find( X509FindType.FindByThumbprint, options.Thumbprint, false );
		} else if ( !string.IsNullOrEmpty( options.SubjectName ) ) {
			certificates = store.Certificates.Find( X509FindType.FindBySubjectName, options.SubjectName, false );
		} else {
			throw new ArgumentException( "Either Thumbprint or SubjectName must be provided for store certificate lookup", nameof( options ) );
		}

		if ( certificates.Count == 0 ) {
			var identifier = !string.IsNullOrEmpty( options.Thumbprint ) ? $"thumbprint '{options.Thumbprint}'" : $"subject '{options.SubjectName}'";
			throw new InvalidOperationException( $"Certificate not found in store with {identifier}" );
		}

		if ( certificates.Count > 1 ) {
			var identifier = !string.IsNullOrEmpty( options.Thumbprint ) ? $"thumbprint '{options.Thumbprint}'" : $"subject '{options.SubjectName}'";
			throw new InvalidOperationException( $"Multiple certificates found in store with {identifier}. Please use a more specific identifier." );
		}

		var certificate = certificates[ 0 ];

		// Verify that the certificate has a private key
		if ( !certificate.HasPrivateKey ) {
			throw new InvalidOperationException( "Certificate found in store does not have an associated private key" );
		}

		return certificate;
	}
}