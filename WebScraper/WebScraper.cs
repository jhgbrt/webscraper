using HtmlAgilityPack;
using ReverseMarkdown;

namespace WebScraper;

public interface IWebScraper
{
    Task<ScrapingResult> ScrapeAsync(string startUrl, string outputDirectory, IProgress<ScrapingProgress>? progress = null);
}

public class WebScraperService : IWebScraper
{
    private readonly HttpClient _httpClient;
    private readonly Converter _converter;

    public WebScraperService(HttpClient? httpClient = null)
    {
        _httpClient = httpClient ?? new HttpClient();
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) WebScraper/1.0");
        _converter = new Converter();
    }

    public async Task<ScrapingResult> ScrapeAsync(string startUrl, string outputDirectory, IProgress<ScrapingProgress>? progress = null)
    {
        var visitedUrls = new HashSet<string>();
        var urlsToVisit = new Queue<string>();
        var errors = new List<string>();

        // Validate and normalize the start URL
        if (!Uri.TryCreate(startUrl, UriKind.Absolute, out var baseUri))
        {
            return new ScrapingResult
            {
                Success = false,
                ErrorMessage = "Invalid URL provided",
                PagesProcessed = 0
            };
        }

        // Create output directory if it doesn't exist
        if (!Directory.Exists(outputDirectory))
        {
            Directory.CreateDirectory(outputDirectory);
        }

        urlsToVisit.Enqueue(startUrl);

        while (urlsToVisit.Count > 0)
        {
            var currentUrl = urlsToVisit.Dequeue();

            if (visitedUrls.Contains(currentUrl))
            {
                continue;
            }

            visitedUrls.Add(currentUrl);

            try
            {
                progress?.Report(new ScrapingProgress
                {
                    CurrentUrl = currentUrl,
                    PagesProcessed = visitedUrls.Count,
                    PagesRemaining = urlsToVisit.Count,
                    Status = "Downloading"
                });

                var html = await _httpClient.GetStringAsync(currentUrl);

                // Convert HTML to Markdown
                var markdown = _converter.Convert(html);

                // Generate a file path from the URL
                var relativePath = FilePathGenerator.GenerateFilePath(currentUrl);
                var filePath = Path.Combine(outputDirectory, relativePath);

                // Ensure directory exists
                var directory = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // Save the markdown file
                await File.WriteAllTextAsync(filePath, markdown);

                progress?.Report(new ScrapingProgress
                {
                    CurrentUrl = currentUrl,
                    PagesProcessed = visitedUrls.Count,
                    PagesRemaining = urlsToVisit.Count,
                    Status = "Saved",
                    FilePath = relativePath
                });

                // Parse HTML to find links
                var doc = new HtmlDocument();
                doc.LoadHtml(html);

                var links = doc.DocumentNode.SelectNodes("//a[@href]");
                if (links != null)
                {
                    foreach (var link in links)
                    {
                        var href = link.GetAttributeValue("href", string.Empty);
                        if (string.IsNullOrWhiteSpace(href))
                        {
                            continue;
                        }

                        // Try to create an absolute URL from the href
                        if (UrlHelper.TryCreateAbsoluteUri(baseUri, href, out var absoluteUri) && absoluteUri != null)
                        {
                            // Only follow links that are on the same domain
                            if (UrlHelper.IsInternalUrl(baseUri, absoluteUri))
                            {
                                var absoluteUrl = UrlHelper.NormalizeUrl(absoluteUri.ToString());

                                if (!visitedUrls.Contains(absoluteUrl) && !urlsToVisit.Contains(absoluteUrl))
                                {
                                    urlsToVisit.Enqueue(absoluteUrl);
                                }
                            }
                        }
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                errors.Add($"Error downloading {currentUrl}: {ex.Message}");
                progress?.Report(new ScrapingProgress
                {
                    CurrentUrl = currentUrl,
                    PagesProcessed = visitedUrls.Count,
                    PagesRemaining = urlsToVisit.Count,
                    Status = "Error",
                    ErrorMessage = ex.Message
                });
            }
            catch (Exception ex)
            {
                errors.Add($"Error processing {currentUrl}: {ex.Message}");
                progress?.Report(new ScrapingProgress
                {
                    CurrentUrl = currentUrl,
                    PagesProcessed = visitedUrls.Count,
                    PagesRemaining = urlsToVisit.Count,
                    Status = "Error",
                    ErrorMessage = ex.Message
                });
            }
        }

        return new ScrapingResult
        {
            Success = true,
            PagesProcessed = visitedUrls.Count,
            Errors = errors
        };
    }
}

public class ScrapingProgress
{
    public string CurrentUrl { get; set; } = string.Empty;
    public int PagesProcessed { get; set; }
    public int PagesRemaining { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? FilePath { get; set; }
    public string? ErrorMessage { get; set; }
}

public class ScrapingResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public int PagesProcessed { get; set; }
    public List<string> Errors { get; set; } = new();
}
