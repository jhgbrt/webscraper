using WebScraper;
using Xunit;

namespace WebScraper.Tests;

public class FilePathGeneratorTests
{
    [Fact]
    public void GenerateFilePath_RootUrl_ReturnsIndexMd()
    {
        // Arrange
        var url = "https://example.com/";

        // Act
        var result = FilePathGenerator.GenerateFilePath(url);

        // Assert
        Assert.Equal("index.md", result);
    }

    [Fact]
    public void GenerateFilePath_SimpleHtmlPage_ReturnsMarkdownPath()
    {
        // Arrange
        var url = "https://example.com/page1.html";

        // Act
        var result = FilePathGenerator.GenerateFilePath(url);

        // Assert
        Assert.Equal("page1.md", result);
    }

    [Fact]
    public void GenerateFilePath_NestedPath_PreservesDirectoryStructure()
    {
        // Arrange
        var url = "https://example.com/docs/guide.html";

        // Act
        var result = FilePathGenerator.GenerateFilePath(url);

        // Assert
        Assert.Equal("docs/guide.md", result);
    }

    [Fact]
    public void GenerateFilePath_DeeplyNestedPath_PreservesDirectoryStructure()
    {
        // Arrange
        var url = "https://example.com/docs/api/v1/reference.html";

        // Act
        var result = FilePathGenerator.GenerateFilePath(url);

        // Assert
        Assert.Equal("docs/api/v1/reference.md", result);
    }

    [Fact]
    public void GenerateFilePath_NoExtension_AddsMarkdownExtension()
    {
        // Arrange
        var url = "https://example.com/docs/guide";

        // Act
        var result = FilePathGenerator.GenerateFilePath(url);

        // Assert
        Assert.Equal("docs/guide.md", result);
    }

    [Fact]
    public void GenerateFilePath_WithQueryString_RemovesQueryString()
    {
        // Arrange
        var url = "https://example.com/page?id=123&ref=abc";

        // Act
        var result = FilePathGenerator.GenerateFilePath(url);

        // Assert
        Assert.Equal("page.md", result);
    }

    [Fact]
    public void GenerateFilePath_WithFragment_RemovesFragment()
    {
        // Arrange
        var url = "https://example.com/page#section1";

        // Act
        var result = FilePathGenerator.GenerateFilePath(url);

        // Assert
        Assert.Equal("page.md", result);
    }

    [Fact]
    public void GenerateFilePath_HtmExtension_ConvertsToMarkdown()
    {
        // Arrange
        var url = "https://example.com/page.htm";

        // Act
        var result = FilePathGenerator.GenerateFilePath(url);

        // Assert
        Assert.Equal("page.md", result);
    }

    [Fact]
    public void GenerateFilePath_TrailingSlash_ReturnsIndexInDirectory()
    {
        // Arrange
        var url = "https://example.com/docs/";

        // Act
        var result = FilePathGenerator.GenerateFilePath(url);

        // Assert
        Assert.Equal("docs/index.md", result);
    }

    [Fact]
    public void GenerateFilePath_ComplexPath_WorksCorrectly()
    {
        // Arrange
        var url = "https://example.com/blog/2024/01/article.html";

        // Act
        var result = FilePathGenerator.GenerateFilePath(url);

        // Assert
        Assert.Equal("blog/2024/01/article.md", result);
    }
}
