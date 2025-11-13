namespace WebScraper;

public static class UrlHelper
{
    /// <summary>
    /// Checks if a URL is internal (same domain) relative to the base URI.
    /// </summary>
    public static bool IsInternalUrl(Uri baseUri, Uri targetUri)
    {
        return targetUri.Host == baseUri.Host;
    }

    /// <summary>
    /// Normalizes a URL by removing fragment identifiers and trailing slashes.
    /// </summary>
    public static string NormalizeUrl(string url)
    {
        // Remove fragment identifiers
        url = url.Split('#')[0];
        return url;
    }

    /// <summary>
    /// Tries to create an absolute URI from a base URI and a relative or absolute href.
    /// </summary>
    public static bool TryCreateAbsoluteUri(Uri baseUri, string href, out Uri? absoluteUri)
    {
        if (string.IsNullOrWhiteSpace(href))
        {
            absoluteUri = null;
            return false;
        }

        return Uri.TryCreate(baseUri, href, out absoluteUri);
    }
}
