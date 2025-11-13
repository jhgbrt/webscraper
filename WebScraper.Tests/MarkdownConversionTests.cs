using ReverseMarkdown;
using Xunit;

namespace WebScraper.Tests;

public class MarkdownConversionTests
{
    [Fact]
    public void Converter_ConvertsHeadingsCorrectly()
    {
        // Arrange
        var converter = new Converter();
        var html = @"<h1>Heading 1</h1>
<h2>Heading 2</h2>
<h3>Heading 3</h3>
<h4>Heading 4</h4>
<h5>Heading 5</h5>
<h6>Heading 6</h6>";

        // Act
        var markdown = converter.Convert(html);

        // Assert
        Assert.Contains("# Heading 1", markdown);
        Assert.Contains("## Heading 2", markdown);
        Assert.Contains("### Heading 3", markdown);
        Assert.Contains("#### Heading 4", markdown);
        Assert.Contains("##### Heading 5", markdown);
        Assert.Contains("###### Heading 6", markdown);
    }

    [Fact]
    public void Converter_ConvertsParagraphsCorrectly()
    {
        // Arrange
        var converter = new Converter();
        var html = @"<p>First paragraph.</p>
<p>Second paragraph with <strong>bold</strong> and <em>italic</em> text.</p>";

        // Act
        var markdown = converter.Convert(html);

        // Assert
        Assert.Contains("First paragraph", markdown);
        Assert.Contains("Second paragraph", markdown);
        Assert.Contains("**bold**", markdown);
        Assert.Contains("*italic*", markdown);
    }

    [Fact]
    public void Converter_ConvertsUnorderedListsCorrectly()
    {
        // Arrange
        var converter = new Converter();
        var html = @"<ul>
<li>First item</li>
<li>Second item</li>
<li>Third item</li>
</ul>";

        // Act
        var markdown = converter.Convert(html);

        // Assert
        Assert.Contains("- First item", markdown);
        Assert.Contains("- Second item", markdown);
        Assert.Contains("- Third item", markdown);
    }

    [Fact]
    public void Converter_ConvertsOrderedListsCorrectly()
    {
        // Arrange
        var converter = new Converter();
        var html = @"<ol>
<li>First step</li>
<li>Second step</li>
<li>Third step</li>
</ol>";

        // Act
        var markdown = converter.Convert(html);

        // Assert
        Assert.Contains("1. First step", markdown);
        Assert.Contains("2. Second step", markdown);
        Assert.Contains("3. Third step", markdown);
    }

    [Fact]
    public void Converter_ConvertsLinksCorrectly()
    {
        // Arrange
        var converter = new Converter();
        var html = @"<p>Visit <a href=""https://example.com"">our website</a> for more info.</p>";

        // Act
        var markdown = converter.Convert(html);

        // Assert
        Assert.Contains("[our website](https://example.com)", markdown);
    }

    [Fact]
    public void Converter_ConvertsImagesCorrectly()
    {
        // Arrange
        var converter = new Converter();
        var html = @"<img src=""image.jpg"" alt=""Sample Image"" />";

        // Act
        var markdown = converter.Convert(html);

        // Assert
        Assert.Contains("![Sample Image](image.jpg)", markdown);
    }

    [Fact]
    public void Converter_ConvertsCodeBlocksCorrectly()
    {
        // Arrange
        var converter = new Converter();
        var html = @"<pre><code>function test() {
    return true;
}</code></pre>";

        // Act
        var markdown = converter.Convert(html);

        // Assert
        Assert.Contains("function test()", markdown);
        Assert.Contains("return true", markdown);
    }

    [Fact]
    public void Converter_ConvertsInlineCodeCorrectly()
    {
        // Arrange
        var converter = new Converter();
        var html = @"<p>Use the <code>console.log()</code> function.</p>";

        // Act
        var markdown = converter.Convert(html);

        // Assert
        Assert.Contains("`console.log()`", markdown);
    }

    [Fact]
    public void Converter_ConvertsBlockquotesCorrectly()
    {
        // Arrange
        var converter = new Converter();
        var html = @"<blockquote>
<p>This is a quote.</p>
</blockquote>";

        // Act
        var markdown = converter.Convert(html);

        // Assert
        Assert.Contains(">", markdown);
        Assert.Contains("This is a quote", markdown);
    }

    [Fact]
    public void Converter_ConvertsTablesCorrectly()
    {
        // Arrange
        var converter = new Converter();
        var html = @"<table>
<thead>
<tr>
<th>Header 1</th>
<th>Header 2</th>
</tr>
</thead>
<tbody>
<tr>
<td>Cell 1</td>
<td>Cell 2</td>
</tr>
</tbody>
</table>";

        // Act
        var markdown = converter.Convert(html);

        // Assert
        Assert.Contains("Header 1", markdown);
        Assert.Contains("Header 2", markdown);
        Assert.Contains("Cell 1", markdown);
        Assert.Contains("Cell 2", markdown);
        Assert.Contains("|", markdown); // Tables use pipe characters
    }

    [Fact]
    public void Converter_HandlesNestedElements()
    {
        // Arrange
        var converter = new Converter();
        var html = @"<ul>
<li><strong>Bold item</strong> with <a href=""link.html"">a link</a></li>
<li><em>Italic item</em> with <code>inline code</code></li>
</ul>";

        // Act
        var markdown = converter.Convert(html);

        // Assert
        Assert.Contains("**Bold item**", markdown);
        Assert.Contains("[a link](link.html)", markdown);
        Assert.Contains("*Italic item*", markdown);
        Assert.Contains("`inline code`", markdown);
    }

    [Fact]
    public void Converter_RemovesScriptAndStyleTags()
    {
        // Arrange
        var converter = new Converter();
        var html = @"<html>
<head>
<style>body { color: red; }</style>
<script>console.log('test');</script>
</head>
<body>
<h1>Content</h1>
<p>Real content here.</p>
</body>
</html>";

        // Act
        var markdown = converter.Convert(html);

        // Assert
        Assert.Contains("# Content", markdown);
        Assert.Contains("Real content here", markdown);
        Assert.DoesNotContain("color: red", markdown);
        Assert.DoesNotContain("console.log", markdown);
    }

    [Fact]
    public void Converter_HandlesComplexDocument()
    {
        // Arrange
        var converter = new Converter();
        var html = @"<!DOCTYPE html>
<html>
<head>
    <title>Documentation</title>
</head>
<body>
    <h1>API Documentation</h1>
    <p>Welcome to the <strong>API documentation</strong>.</p>
    
    <h2>Getting Started</h2>
    <p>Follow these steps:</p>
    <ol>
        <li>Install the package</li>
        <li>Configure your app</li>
        <li>Start using the API</li>
    </ol>
    
    <h2>Example</h2>
    <pre><code>const api = require('api');
api.connect();</code></pre>
    
    <h2>Resources</h2>
    <ul>
        <li><a href=""https://docs.example.com"">Full Documentation</a></li>
        <li><a href=""https://github.com/example/api"">GitHub Repository</a></li>
    </ul>
</body>
</html>";

        // Act
        var markdown = converter.Convert(html);

        // Assert
        Assert.Contains("# API Documentation", markdown);
        Assert.Contains("## Getting Started", markdown);
        Assert.Contains("## Example", markdown);
        Assert.Contains("## Resources", markdown);
        Assert.Contains("**API documentation**", markdown);
        Assert.Contains("1. Install the package", markdown);
        Assert.Contains("2. Configure your app", markdown);
        Assert.Contains("3. Start using the API", markdown);
        Assert.Contains("const api = require('api')", markdown);
        Assert.Contains("[Full Documentation](https://docs.example.com)", markdown);
        Assert.Contains("[GitHub Repository](https://github.com/example/api)", markdown);
    }
}
