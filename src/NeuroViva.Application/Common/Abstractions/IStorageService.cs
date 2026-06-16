namespace NeuroViva.Application.Common.Abstractions;

public interface IStorageService
{
    Task<string> UploadAsync(string bucket, string path, Stream content, string contentType, CancellationToken ct = default);
    Task DeleteAsync(string bucket, string path, CancellationToken ct = default);
    string GetPublicUrl(string bucket, string path);
}
