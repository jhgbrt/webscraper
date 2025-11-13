namespace WebScraper;

public static class FilePathGenerator
{
    /// <summary>
    /// Generates a file path from a URL, using directory structure instead of underscores.
    /// For example: https://example.com/docs/guide.html -> docs/guide.md
    /// </summary>
    public static string GenerateFilePath(string url)
    {
        var uri = new Uri(url);
        var path = uri.AbsolutePath;

        // Remove leading slash
        if (path.StartsWith("/"))
        {
            path = path.Substring(1);
        }

        // If path is empty or root, use index
        if (string.IsNullOrEmpty(path) || path == "/")
        {
            path = "index";
        }

        // Remove query strings and fragments
        path = path.Split('?')[0].Split('#')[0];

        // Replace invalid filename characters, but preserve directory separators
        var invalidChars = Path.GetInvalidFileNameChars();
        // Split by directory separator to handle each part individually
        var parts = path.Split('/');
        for (int i = 0; i < parts.Length; i++)
        {
            foreach (var c in invalidChars)
            {
                // Don't replace the directory separator
                if (c != '/' && c != '\\')
                {
                    parts[i] = parts[i].Replace(c, '_');
                }
            }
        }
        path = string.Join("/", parts);

        // Ensure .md extension
        if (!path.EndsWith(".md", StringComparison.OrdinalIgnoreCase))
        {
            // If it has an HTML extension, replace it
            if (path.EndsWith(".html", StringComparison.OrdinalIgnoreCase) || 
                path.EndsWith(".htm", StringComparison.OrdinalIgnoreCase))
            {
                path = path.Substring(0, path.LastIndexOf('.'));
            }
            else if (path.EndsWith("/"))
            {
                // If path ends with /, append index
                path = path + "index";
            }
            path += ".md";
        }

        return path;
    }
}
