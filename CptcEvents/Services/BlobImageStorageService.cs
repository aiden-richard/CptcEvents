using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Http;

namespace CptcEvents.Services;

/// <summary>
/// Azure Blob Storage implementation for image upload and deletion.
/// </summary>
public class BlobImageStorageService : IImageStorageService
{
    private readonly BlobServiceClient _blobServiceClient;

    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".gif", ".webp"
    };

    private static readonly Dictionary<string, string> ContentTypeMappings = new(StringComparer.OrdinalIgnoreCase)
    {
        { ".jpg", "image/jpeg" },
        { ".jpeg", "image/jpeg" },
        { ".png", "image/png" },
        { ".gif", "image/gif" },
        { ".webp", "image/webp" }
    };

    private const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5 MB

    public BlobImageStorageService(BlobServiceClient blobServiceClient)
    {
        _blobServiceClient = blobServiceClient;
    }

    public async Task<string> UploadImageAsync(IFormFile file, string containerName)
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("File is empty or null.", nameof(file));

        if (file.Length > MaxFileSizeBytes)
            throw new ArgumentException($"File size exceeds the maximum allowed size of {MaxFileSizeBytes / (1024 * 1024)} MB.");

        var extension = Path.GetExtension(file.FileName);
        if (string.IsNullOrEmpty(extension) || !AllowedExtensions.Contains(extension))
            throw new ArgumentException($"File type '{extension}' is not allowed. Allowed types: {string.Join(", ", AllowedExtensions)}");

        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        // NOTE: The container is created with PublicAccessType.Blob so that uploaded images
        // are publicly accessible without authentication. Do not use this service for
        // sensitive or private images. If restricted access is required, configure the
        // container with PublicAccessType.None and expose images via SAS tokens or other
        // access-control mechanisms instead.
        await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

        var blobName = $"{Guid.NewGuid()}{extension.ToLowerInvariant()}";
        var blobClient = containerClient.GetBlobClient(blobName);

        var contentType = ContentTypeMappings.GetValueOrDefault(extension, "application/octet-stream");

        using var stream = file.OpenReadStream();
        await blobClient.UploadAsync(stream, new BlobHttpHeaders
        {
            ContentType = contentType
        });

        return blobClient.Uri.ToString();
    }

    public async Task DeleteImageAsync(string imageUrl)
    {
        if (string.IsNullOrEmpty(imageUrl))
            throw new ArgumentException("Image URL is null or empty.", nameof(imageUrl));

        try
        {
            var uri = new Uri(imageUrl);
            // Path segments: /{container}/{blobName}
            var segments = uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (segments.Length < 2)
                return;

            var containerName = segments[0];
            var blobName = string.Join('/', segments.Skip(1));

            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(blobName);
            await blobClient.DeleteIfExistsAsync();
        }
        catch (UriFormatException)
        {
            // Invalid URL, nothing to delete
        }
    }
}
