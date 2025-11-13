using System.Net;
using WebScraper;
using Xunit;

namespace WebScraper.Tests;

public class WebScraperServiceTests
{
    [Fact]
    public async Task ScrapeAsync_WithInvalidUrl_ReturnsFailureResult()
    {
        // Arrange
        var scraper = new WebScraperService();
        var outputDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        try
        {
            // Act
            var result = await scraper.ScrapeAsync("not-a-valid-url", outputDir);

            // Assert
            Assert.False(result.Success);
            Assert.NotNull(result.ErrorMessage);
            Assert.Equal(0, result.PagesProcessed);
        }
        finally
        {
            // Cleanup
            if (Directory.Exists(outputDir))
            {
                Directory.Delete(outputDir, true);
            }
        }
    }

    [Fact]
    public async Task ScrapeAsync_VisitsInternalLinksOnly()
    {
        // Arrange
        var html = @"
<!DOCTYPE html>
<html>
<body>
    <a href=""page2.html"">Internal Page</a>
    <a href=""https://external.com/page"">External Link</a>
    <a href=""https://example.com/page3.html"">Another Internal</a>
</body>
</html>";

        var handler = new MockHttpMessageHandler();
        handler.AddResponse("https://example.com/", html);
        handler.AddResponse("https://example.com/page2.html", "<html><body>Page 2</body></html>");
        handler.AddResponse("https://example.com/page3.html", "<html><body>Page 3</body></html>");

        var httpClient = new HttpClient(handler);
        var scraper = new WebScraperService(httpClient);
        var outputDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        try
        {
            // Act
            var result = await scraper.ScrapeAsync("https://example.com/", outputDir);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(3, result.PagesProcessed); // Should process: /, page2.html, page3.html
            Assert.Equal(3, handler.RequestedUrls.Count);
            Assert.Contains("https://example.com/", handler.RequestedUrls);
            Assert.Contains("https://example.com/page2.html", handler.RequestedUrls);
            Assert.Contains("https://example.com/page3.html", handler.RequestedUrls);
            Assert.DoesNotContain("https://external.com/page", handler.RequestedUrls);
        }
        finally
        {
            // Cleanup
            if (Directory.Exists(outputDir))
            {
                Directory.Delete(outputDir, true);
            }
        }
    }

    [Fact]
    public async Task ScrapeAsync_SkipsAlreadyVisitedUrls()
    {
        // Arrange
        var html = @"
<!DOCTYPE html>
<html>
<body>
    <a href=""page2.html"">Page 2</a>
</body>
</html>";

        var page2Html = @"
<!DOCTYPE html>
<html>
<body>
    <a href=""/"">Back to Home</a>
</body>
</html>";

        var handler = new MockHttpMessageHandler();
        handler.AddResponse("https://example.com/", html);
        handler.AddResponse("https://example.com/page2.html", page2Html);

        var httpClient = new HttpClient(handler);
        var scraper = new WebScraperService(httpClient);
        var outputDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        try
        {
            // Act
            var result = await scraper.ScrapeAsync("https://example.com/", outputDir);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(2, result.PagesProcessed); // Should process: / and page2.html only once each
            Assert.Equal(2, handler.RequestedUrls.Count);
        }
        finally
        {
            // Cleanup
            if (Directory.Exists(outputDir))
            {
                Directory.Delete(outputDir, true);
            }
        }
    }

    [Fact]
    public async Task ScrapeAsync_CreatesDirectoryStructure()
    {
        // Arrange
        var html = @"
<!DOCTYPE html>
<html>
<body>
    <a href=""docs/guide.html"">Guide</a>
</body>
</html>";

        var guideHtml = "<html><body>Guide Content</body></html>";

        var handler = new MockHttpMessageHandler();
        handler.AddResponse("https://example.com/", html);
        handler.AddResponse("https://example.com/docs/guide.html", guideHtml);

        var httpClient = new HttpClient(handler);
        var scraper = new WebScraperService(httpClient);
        var outputDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        try
        {
            // Act
            var result = await scraper.ScrapeAsync("https://example.com/", outputDir);

            // Assert
            Assert.True(result.Success);
            Assert.True(File.Exists(Path.Combine(outputDir, "index.md")));
            Assert.True(File.Exists(Path.Combine(outputDir, "docs", "guide.md")));
        }
        finally
        {
            // Cleanup
            if (Directory.Exists(outputDir))
            {
                Directory.Delete(outputDir, true);
            }
        }
    }

    [Fact]
    public async Task ScrapeAsync_HandlesFragmentIdentifiers()
    {
        // Arrange
        var html = @"
<!DOCTYPE html>
<html>
<body>
    <a href=""page2.html#section1"">Page 2 Section 1</a>
    <a href=""page2.html#section2"">Page 2 Section 2</a>
</body>
</html>";

        var handler = new MockHttpMessageHandler();
        handler.AddResponse("https://example.com/", html);
        handler.AddResponse("https://example.com/page2.html", "<html><body>Page 2</body></html>");

        var httpClient = new HttpClient(handler);
        var scraper = new WebScraperService(httpClient);
        var outputDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        try
        {
            // Act
            var result = await scraper.ScrapeAsync("https://example.com/", outputDir);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(2, result.PagesProcessed); // Should process / and page2.html once (not twice)
            Assert.Equal(2, handler.RequestedUrls.Count);
        }
        finally
        {
            // Cleanup
            if (Directory.Exists(outputDir))
            {
                Directory.Delete(outputDir, true);
            }
        }
    }
}

// Mock HTTP handler for testing
public class MockHttpMessageHandler : HttpMessageHandler
{
    private readonly Dictionary<string, string> _responses = new();
    public List<string> RequestedUrls { get; } = new();

    public void AddResponse(string url, string html)
    {
        _responses[url] = html;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var url = request.RequestUri?.ToString() ?? string.Empty;
        RequestedUrls.Add(url);

        if (_responses.TryGetValue(url, out var html))
        {
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(html)
            });
        }

        return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
    }
}
