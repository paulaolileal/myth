using Microsoft.AspNetCore.Http;
using Myth.Builders;
using Myth.Models;

namespace Myth.Interfaces;

/// <summary>
/// Interface for REST HTTP actions
/// </summary>
public interface IRestRequest {

	/// <summary>
	/// Perform GET request
	/// </summary>
	/// <param name="url">The URL</param>
	/// <returns>Post-processing interface</returns>
	IRestPostProcessing DoGet( string url );

	/// <summary>
	/// Perform POST request
	/// </summary>
	/// <typeparam name="TBody">Request body type</typeparam>
	/// <param name="url">The URL</param>
	/// <param name="body">Request body</param>
	/// <returns>Post-processing interface</returns>
	IRestPostProcessing DoPost<TBody>( string url, TBody? body = default );

	/// <summary>
	/// Perform PUT request
	/// </summary>
	/// <typeparam name="TBody">Request body type</typeparam>
	/// <param name="url">The URL</param>
	/// <param name="body">Request body</param>
	/// <returns>Post-processing interface</returns>
	IRestPostProcessing DoPut<TBody>( string url, TBody? body = default );

	/// <summary>
	/// Perform DELETE request
	/// </summary>
	/// <param name="url">The URL</param>
	/// <returns>Post-processing interface</returns>
	IRestPostProcessing DoDelete( string url );

	/// <summary>
	/// Perform PATCH request
	/// </summary>
	/// <typeparam name="TBody">Request body type</typeparam>
	/// <param name="url">The URL</param>
	/// <param name="body">Request body</param>
	/// <returns>Post-processing interface</returns>
	IRestPostProcessing DoPatch<TBody>( string url, TBody? body = default );

	/// <summary>
	/// Download file
	/// </summary>
	/// <param name="url">The URL</param>
	/// <returns>Post-processing interface</returns>
	IRestPostProcessing DoDownload( string url );

	/// <summary>
	/// Upload file
	/// </summary>
	/// <typeparam name="T">Content type</typeparam>
	/// <param name="url">The URL</param>
	/// <param name="body">File content</param>
	/// <param name="contentType">Content type</param>
	/// <param name="settings">Upload settings</param>
	/// <returns>Post-processing interface</returns>
	IRestPostProcessing DoUpload<T>( string url, T body, string contentType, Action<RestUploadSettings>? settings = null );

	/// <summary>
	/// Upload file from stream
	/// </summary>
	/// <param name="url">The URL</param>
	/// <param name="stream">File stream</param>
	/// <param name="contentType">Content type</param>
	/// <param name="settings">Upload settings</param>
	/// <returns>Post-processing interface</returns>
	IRestPostProcessing DoUpload( string url, Stream stream, string contentType, Action<RestUploadSettings>? settings = null );

	/// <summary>
	/// Upload form file
	/// </summary>
	/// <param name="url">The URL</param>
	/// <param name="file">Form file</param>
	/// <param name="settings">Upload settings</param>
	/// <returns>Post-processing interface</returns>
	IRestPostProcessing DoUpload( string url, IFormFile file, Action<RestUploadSettings>? settings = null );

	/// <summary>
	/// Upload HTTP content
	/// </summary>
	/// <param name="url">The URL</param>
	/// <param name="content">HTTP content</param>
	/// <param name="settings">Upload settings</param>
	/// <returns>Post-processing interface</returns>
	IRestPostProcessing DoUpload( string url, HttpContent content, Action<RestUploadSettings>? settings = null );

	/// <summary>
	/// Configure REST client settings
	/// </summary>
	/// <param name="configurationBuilder">Configuration action</param>
	/// <returns>REST actions interface</returns>
	IRestRequest Configure( Action<ConfigurationBuilder> configurationBuilder );
}
