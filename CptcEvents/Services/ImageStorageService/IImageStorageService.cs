using Microsoft.AspNetCore.Http;

namespace CptcEvents.Services;

/// <summary>
/// Service interface for uploading and deleting images in cloud storage.
/// </summary>
public interface IImageStorageService
{
    /// <summary>
    /// Uploads an image file to the specified container and returns the public URL.
    /// </summary>
    /// <param name="file">The image file to upload.</param>
    /// <param name="containerName">The storage container name.</param>
    /// <returns>The public URL of the uploaded image.</returns>
    Task<string> UploadImageAsync(IFormFile file, string containerName);

    /// <summary>
    /// Deletes an image from storage by its URL.
    /// </summary>
    /// <param name="imageUrl">The URL of the image to delete.</param>
    Task DeleteImageAsync(string imageUrl);

    /// <summary>
    /// Downloads an image stream from storage by its URL.
    /// </summary>
    /// <param name="imageUrl">The URL of the image to download.</param>
    /// <returns>A tuple containing the stream and content type, or null if the image is not found.</returns>
    Task<(Stream Content, string ContentType)?> GetImageStreamAsync(string imageUrl);
}
