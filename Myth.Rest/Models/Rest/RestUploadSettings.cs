using System.Diagnostics.CodeAnalysis;

namespace Myth.Models.Rest;

[ExcludeFromCodeCoverage]
public class RestUploadSettings {

	public enum UploadMethod {
		POST,
		PUT,
		PATCH
	}

	internal UploadMethod Method { get; private set; }

	/// <summary>
	/// Use `POST` as method for upload
	/// </summary>
	public void UsePostAsMethod( ) => Method = UploadMethod.POST;

	/// <summary>
	/// Use `PUT` as method for upload
	/// </summary>
	public void UsePutAsMethod( ) => Method = UploadMethod.PUT;

	/// <summary>
	/// Use `PATCH` as method for upload
	/// </summary>
	public void UsePatchAsMethod( ) => Method = UploadMethod.PATCH;
}