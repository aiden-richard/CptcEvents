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
        // NOTE: The container is created with PublicAccessType.None (private). Image access
        // is controlled through the ImagesController proxy which enforces authorization rules
        // based on event visibility and group membership.
        await containerClient.CreateIfNotExistsAsync(PublicAccessType.None);

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

    public async Task<(Stream Content, string ContentType)?> GetImageStreamAsync(string imageUrl)
    {
        if (string.IsNullOrEmpty(imageUrl))
            return null;

        try
        {
            var uri = new Uri(imageUrl);
            // Path segments: /{container}/{blobName}
            var segments = uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (segments.Length < 2)
                return null;

            var containerName = segments[0];
            var blobName = string.Join('/', segments.Skip(1));

            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(blobName);

            var response = await blobClient.DownloadStreamingAsync();
            var contentType = response.Value.Details.ContentType ?? "application/octet-stream";

            return (response.Value.Content, contentType);
        }
        catch (Azure.RequestFailedException ex) when (ex.Status == 404)
        {
            // Blob not found
            return null;
        }
        catch (UriFormatException)
        {
            // Invalid URL
            return null;
        }
    }
}
