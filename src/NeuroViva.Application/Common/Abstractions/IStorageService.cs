namespace NeuroViva.Application.Common.Abstractions;

public interface IStorageService
{
    /// <summary>Uploads to a private bucket. Returns the storage path (same as input path on success).</summary>
    Task<string> UploadAsync(string bucket, string path, Stream content, string contentType, CancellationToken ct = default);

    /// <summary>Mints a short-lived signed URL to read a private object. Returns null if the path is null/empty.</summary>
    Task<string?> GetSignedUrlAsync(string bucket, string path, TimeSpan expiry, CancellationToken ct = default);

    /// <summary>Downloads the raw bytes of an object from a private bucket.</summary>
    Task<byte[]> DownloadAsync(string bucket, string path, CancellationToken ct = default);

    Task DeleteAsync(string bucket, string path, CancellationToken ct = default);
}
