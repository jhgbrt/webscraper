using WebScraper;
using Xunit;

namespace WebScraper.Tests;

public class UrlHelperTests
{
    [Fact]
    public void IsInternalUrl_SameDomain_ReturnsTrue()
    {
        // Arrange
        var baseUri = new Uri("https://example.com/page1");
        var targetUri = new Uri("https://example.com/page2");

        // Act
        var result = UrlHelper.IsInternalUrl(baseUri, targetUri);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsInternalUrl_DifferentDomain_ReturnsFalse()
    {
        // Arrange
        var baseUri = new Uri("https://example.com/page1");
        var targetUri = new Uri("https://external.com/page2");

        // Act
        var result = UrlHelper.IsInternalUrl(baseUri, targetUri);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsInternalUrl_Subdomain_ReturnsFalse()
    {
        // Arrange
        var baseUri = new Uri("https://example.com/page1");
        var targetUri = new Uri("https://blog.example.com/page2");

        // Act
        var result = UrlHelper.IsInternalUrl(baseUri, targetUri);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void NormalizeUrl_RemovesFragmentIdentifier()
    {
        // Arrange
        var url = "https://example.com/page#section1";

        // Act
        var result = UrlHelper.NormalizeUrl(url);

        // Assert
        Assert.Equal("https://example.com/page", result);
    }

    [Fact]
    public void NormalizeUrl_NoFragment_ReturnsUnchanged()
    {
        // Arrange
        var url = "https://example.com/page";

        // Act
        var result = UrlHelper.NormalizeUrl(url);

        // Assert
        Assert.Equal("https://example.com/page", result);
    }

    [Fact]
    public void TryCreateAbsoluteUri_WithValidRelativeUrl_ReturnsTrue()
    {
        // Arrange
        var baseUri = new Uri("https://example.com/docs/");
        var href = "guide.html";

        // Act
        var result = UrlHelper.TryCreateAbsoluteUri(baseUri, href, out var absoluteUri);

        // Assert
        Assert.True(result);
        Assert.NotNull(absoluteUri);
        Assert.Equal("https://example.com/docs/guide.html", absoluteUri!.ToString());
    }

    [Fact]
    public void TryCreateAbsoluteUri_WithValidAbsoluteUrl_ReturnsTrue()
    {
        // Arrange
        var baseUri = new Uri("https://example.com/");
        var href = "https://example.com/page";

        // Act
        var result = UrlHelper.TryCreateAbsoluteUri(baseUri, href, out var absoluteUri);

        // Assert
        Assert.True(result);
        Assert.NotNull(absoluteUri);
        Assert.Equal("https://example.com/page", absoluteUri!.ToString());
    }

    [Fact]
    public void TryCreateAbsoluteUri_WithEmptyString_ReturnsFalse()
    {
        // Arrange
        var baseUri = new Uri("https://example.com/");
        var href = "";

        // Act
        var result = UrlHelper.TryCreateAbsoluteUri(baseUri, href, out var absoluteUri);

        // Assert
        Assert.False(result);
        Assert.Null(absoluteUri);
    }

    [Fact]
    public void TryCreateAbsoluteUri_WithNull_ReturnsFalse()
    {
        // Arrange
        var baseUri = new Uri("https://example.com/");
        string? href = null;

        // Act
        var result = UrlHelper.TryCreateAbsoluteUri(baseUri, href!, out var absoluteUri);

        // Assert
        Assert.False(result);
        Assert.Null(absoluteUri);
    }
}
