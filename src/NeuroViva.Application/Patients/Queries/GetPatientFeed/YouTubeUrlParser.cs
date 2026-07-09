using System.Text.RegularExpressions;

namespace NeuroViva.Application.Patients.Queries.GetPatientFeed;

internal static class YouTubeUrlParser
{
    private static readonly Regex VideoIdRegex =
        new(@"^[A-Za-z0-9_-]{11}$", RegexOptions.Compiled);

    /// <summary>
    /// Parses a YouTube URL and returns the standard embed URL, or null if not a
    /// recognizable YouTube URL or the video ID does not pass validation.
    /// Does not throw.
    /// </summary>
    public static string? TryGetEmbedUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return null;

        if (!Uri.TryCreate(url.Trim(), UriKind.Absolute, out var uri))
            return null;

        var host = uri.Host.ToLowerInvariant();

        // youtu.be/{id}
        if (host == "youtu.be")
        {
            var id = uri.AbsolutePath.TrimStart('/');
            return IsValidVideoId(id) ? $"https://www.youtube.com/embed/{id}" : null;
        }

        // *.youtube.com
        if (!host.EndsWith("youtube.com", StringComparison.OrdinalIgnoreCase))
            return null;

        var path = uri.AbsolutePath.TrimStart('/');

        // youtube.com/embed/{id}  — already an embed URL
        if (path.StartsWith("embed/", StringComparison.OrdinalIgnoreCase))
        {
            var id = path["embed/".Length..].Split('/')[0];
            return IsValidVideoId(id) ? $"https://www.youtube.com/embed/{id}" : null;
        }

        // youtube.com/shorts/{id}
        if (path.StartsWith("shorts/", StringComparison.OrdinalIgnoreCase))
        {
            var id = path["shorts/".Length..].Split('/')[0];
            return IsValidVideoId(id) ? $"https://www.youtube.com/embed/{id}" : null;
        }

        // youtube.com/watch?v={id}
        if (path.Equals("watch", StringComparison.OrdinalIgnoreCase))
        {
            var id = ParseQueryParam(uri.Query, "v");
            return IsValidVideoId(id) ? $"https://www.youtube.com/embed/{id}" : null;
        }

        return null;
    }

    private static bool IsValidVideoId(string? id)
        => !string.IsNullOrEmpty(id) && VideoIdRegex.IsMatch(id);

    /// <summary>
    /// Extracts the value of a named parameter from a URI query string without
    /// introducing a dependency on System.Web.
    /// </summary>
    private static string? ParseQueryParam(string query, string paramName)
    {
        var q = query.TrimStart('?');
        foreach (var segment in q.Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var idx = segment.IndexOf('=');
            if (idx <= 0) continue;
            var key = Uri.UnescapeDataString(segment[..idx]);
            if (string.Equals(key, paramName, StringComparison.OrdinalIgnoreCase))
                return Uri.UnescapeDataString(segment[(idx + 1)..]);
        }
        return null;
    }
}
