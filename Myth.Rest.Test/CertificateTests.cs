using System;
using System.Net.Http;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using FluentAssertions;
using Myth.Helpers;
using Myth.Models;
using Myth.Rest.Test.Base;
using Xunit;

namespace Myth.Rest.Test;

/// <summary>
/// Tests for certificate functionality
/// </summary>
public class CertificateTests : BaseTests {

	[Fact]
	public void WithCertificate_X509Certificate2_ShouldConfigureCertificateOptions( ) {
		// Arrange
		using var certificate = CreateTestCertificate( );

		// Act & Assert
		var action = ( ) => Rest.Create( )
			.Configure( config => config.WithCertificate( certificate ) );

		action.Should( ).NotThrow( );
	}

	[Fact]
	public void WithCertificate_NullCertificate_ShouldThrowArgumentNullException( ) {
		// Act & Assert
		var action = ( ) => Rest.Create( )
			.Configure( config => config.WithCertificate( ( X509Certificate2 )null! ) );

		action.Should( ).Throw<ArgumentNullException>( )
			.WithParameterName( "certificate" );
	}

	[Fact]
	public void WithCertificate_PfxPath_ShouldConfigureCertificateOptions( ) {
		// Arrange
		var pfxPath = "test.pfx";
		var password = "password123";

		// Act & Assert
		var action = ( ) => Rest.Create( )
			.Configure( config => config.WithCertificate( pfxPath, password ) );

		action.Should( ).NotThrow( );
	}

	[Fact]
	public void WithCertificate_EmptyPfxPath_ShouldThrowArgumentException( ) {
		// Act & Assert
		var action = ( ) => Rest.Create( )
			.Configure( config => config.WithCertificate( "", "password" ) );

		action.Should( ).Throw<ArgumentException>( )
			.WithParameterName( "pfxPath" );
	}

	[Fact]
	public void WithCertificate_EmptyPassword_ShouldThrowArgumentException( ) {
		// Act & Assert
		var action = ( ) => Rest.Create( )
			.Configure( config => config.WithCertificate( "test.pfx", "" ) );

		action.Should( ).Throw<ArgumentException>( )
			.WithParameterName( "password" );
	}

	[Fact]
	public void WithCertificate_PfxData_ShouldConfigureCertificateOptions( ) {
		// Arrange
		var pfxData = new byte[ ] { 1, 2, 3, 4, 5 };
		var password = "password123";

		// Act & Assert
		var action = ( ) => Rest.Create( )
			.Configure( config => config.WithCertificate( pfxData, password ) );

		action.Should( ).NotThrow( );
	}

	[Fact]
	public void WithCertificate_NullPfxData_ShouldThrowArgumentNullException( ) {
		// Act & Assert
		var action = ( ) => Rest.Create( )
			.Configure( config => config.WithCertificate( ( byte[ ] )null!, "password" ) );

		action.Should( ).Throw<ArgumentNullException>( )
			.WithParameterName( "pfxData" );
	}

	[Fact]
	public void WithCertificate_PemFiles_ShouldConfigureCertificateOptions( ) {
		// Arrange
		var certPath = "cert.pem";
		var keyPath = "key.pem";

		// Act & Assert
		var action = ( ) => Rest.Create( )
			.Configure( config => config.WithCertificate( certPath, keyPath ) );

		action.Should( ).NotThrow( );
	}

	[Fact]
	public void WithCertificate_PemFilesWithPassword_ShouldConfigureCertificateOptions( ) {
		// Arrange
		var certPath = "cert.pem";
		var keyPath = "key.pem";
		var password = "keyPassword";

		// Act & Assert
		var action = ( ) => Rest.Create( )
			.Configure( config => config.WithCertificate( certPath, keyPath, password ) );

		action.Should( ).NotThrow( );
	}

	[Fact]
	public void WithCertificate_EmptyPemCertPath_ShouldThrowArgumentException( ) {
		// Act & Assert
		var action = ( ) => Rest.Create( )
			.Configure( config => config.WithCertificate( "", "key.pem", null ) );

		action.Should( ).Throw<ArgumentException>( )
			.WithParameterName( "certificatePath" );
	}

	[Fact]
	public void WithCertificate_EmptyPemKeyPath_ShouldThrowArgumentException( ) {
		// Act & Assert
		var action = ( ) => Rest.Create( )
			.Configure( config => config.WithCertificate( "cert.pem", "", null ) );

		action.Should( ).Throw<ArgumentException>( )
			.WithParameterName( "keyPath" );
	}

	[Fact]
	public void WithCertificateFromStore_Thumbprint_ShouldConfigureCertificateOptions( ) {
		// Arrange
		var thumbprint = "ABC123DEF456";

		// Act & Assert
		var action = ( ) => Rest.Create( )
			.Configure( config => config.WithCertificateFromStore( thumbprint ) );

		action.Should( ).NotThrow( );
	}

	[Fact]
	public void WithCertificateFromStore_EmptyThumbprint_ShouldThrowArgumentException( ) {
		// Act & Assert
		var action = ( ) => Rest.Create( )
			.Configure( config => config.WithCertificateFromStore( "" ) );

		action.Should( ).Throw<ArgumentException>( )
			.WithParameterName( "thumbprint" );
	}

	[Fact]
	public void WithCertificateFromStoreBySubject_SubjectName_ShouldConfigureCertificateOptions( ) {
		// Arrange
		var subjectName = "CN=Test Certificate";

		// Act & Assert
		var action = ( ) => Rest.Create( )
			.Configure( config => config.WithCertificateFromStoreBySubject( subjectName ) );

		action.Should( ).NotThrow( );
	}

	[Fact]
	public void WithCertificateFromStoreBySubject_EmptySubjectName_ShouldThrowArgumentException( ) {
		// Act & Assert
		var action = ( ) => Rest.Create( )
			.Configure( config => config.WithCertificateFromStoreBySubject( "" ) );

		action.Should( ).Throw<ArgumentException>( )
			.WithParameterName( "subjectName" );
	}

	[Fact]
	public void WithCertificate_AdvancedConfiguration_ShouldConfigureCertificateOptions( ) {
		// Act & Assert
		var action = ( ) => Rest.Create( )
			.Configure( config => config.WithCertificate( options => {
				options.Type = CertificateType.X509Certificate2;
				options.Certificate = CreateTestCertificate( );
				options.ValidateServerCertificate = false;
			} ) );

		action.Should( ).NotThrow( );
	}

	[Fact]
	public void WithCertificate_NullAdvancedConfiguration_ShouldThrowArgumentNullException( ) {
		// Act & Assert
		var action = ( ) => Rest.Create( )
			.Configure( config => config.WithCertificate( ( Action<CertificateOptions> )null! ) );

		action.Should( ).Throw<ArgumentNullException>( )
			.WithParameterName( "configure" );
	}

	[Fact]
	public void CertificateHelper_LoadCertificate_NullOptions_ShouldThrowArgumentNullException( ) {
		// Act & Assert
		var action = ( ) => CertificateHelper.LoadCertificate( null! );

		action.Should( ).Throw<ArgumentNullException>( )
			.WithParameterName( "options" );
	}

	[Fact]
	public void CertificateHelper_LoadCertificate_X509Certificate2Type_ShouldReturnCertificate( ) {
		// Arrange
		using var expectedCertificate = CreateTestCertificate( );
		var options = new CertificateOptions {
			Type = CertificateType.X509Certificate2,
			Certificate = expectedCertificate
		};

		// Act
		var result = CertificateHelper.LoadCertificate( options );

		// Assert
		result.Should( ).Be( expectedCertificate );
	}

	[Fact]
	public void CertificateHelper_LoadCertificate_X509Certificate2TypeWithNullCertificate_ShouldThrowArgumentException( ) {
		// Arrange
		var options = new CertificateOptions {
			Type = CertificateType.X509Certificate2,
			Certificate = null
		};

		// Act & Assert
		var action = ( ) => CertificateHelper.LoadCertificate( options );

		action.Should( ).Throw<ArgumentException>( )
			.WithMessage( "*Certificate instance is required for X509Certificate2 type*" );
	}

	[Fact]
	public void CertificateHelper_ConfigureHandler_NullHandler_ShouldThrowArgumentNullException( ) {
		// Arrange
		var options = new CertificateOptions {
			Type = CertificateType.X509Certificate2,
			Certificate = CreateTestCertificate( )
		};

		// Act & Assert
		var action = ( ) => CertificateHelper.ConfigureHandler( null!, options );

		action.Should( ).Throw<ArgumentNullException>( )
			.WithParameterName( "handler" );
	}

	[Fact]
	public void CertificateHelper_ConfigureHandler_NullOptions_ShouldThrowArgumentNullException( ) {
		// Arrange
		using var handler = new HttpClientHandler( );

		// Act & Assert
		var action = ( ) => CertificateHelper.ConfigureHandler( handler, null! );

		action.Should( ).Throw<ArgumentNullException>( )
			.WithParameterName( "options" );
	}

	[Fact]
	public void CertificateHelper_ConfigureHandler_ValidOptions_ShouldAddCertificateToHandler( ) {
		// Arrange
		using var handler = new HttpClientHandler( );
		using var certificate = CreateTestCertificate( );
		var options = new CertificateOptions {
			Type = CertificateType.X509Certificate2,
			Certificate = certificate
		};

		// Act
		CertificateHelper.ConfigureHandler( handler, options );

		// Assert
		handler.ClientCertificates.Count.Should( ).Be( 1 );
		handler.ClientCertificates[ 0 ].Should( ).Be( certificate );
	}

	/// <summary>
	/// Create a test certificate for testing purposes
	/// </summary>
	/// <returns>Test X509Certificate2</returns>
	private static X509Certificate2 CreateTestCertificate( ) {
		// Create a simple self-signed certificate for testing
		// This is just for unit testing - not for production use
		using var rsa = RSA.Create( 2048 );
		var req = new CertificateRequest( "CN=Test", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1 );
		var cert = req.CreateSelfSigned( DateTimeOffset.Now, DateTimeOffset.Now.AddYears( 1 ) );

		return cert;
	}
}
