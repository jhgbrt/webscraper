# WebScraper

A .NET 9.0 console application that downloads HTML pages from a website and converts them to Markdown format. It automatically follows internal links and saves all pages as Markdown files.

## Features

- Downloads HTML content from any URL
- Converts HTML to clean Markdown format
- Automatically follows all internal site links
- Tracks visited URLs to avoid duplicates
- Beautiful console UI with progress reporting (using Spectre.Console)
- Saves each page as a separate Markdown file

## Dependencies

- **Spectre.Console** - Beautiful console UI and progress reporting
- **HtmlAgilityPack** - HTML parsing and link extraction
- **ReverseMarkdown** - HTML to Markdown conversion

## Installation

```bash
cd WebScraper
dotnet build
```

## Usage

```bash
# Basic usage - scrape a website to the default 'output' directory
dotnet run -- <url>

# Specify custom output directory
dotnet run -- <url> --output <directory>
dotnet run -- <url> -o <directory>

# Show help
dotnet run -- --help
```

## Examples

```bash
# Scrape a website to the default 'output' directory
dotnet run -- https://example.com

# Scrape a website to a custom directory
dotnet run -- https://example.com -o my-markdown-files

# Scrape a local development server
dotnet run -- http://localhost:8080
```

## How It Works

1. The scraper starts with the provided URL
2. Downloads the HTML content
3. Converts the HTML to Markdown using ReverseMarkdown
4. Extracts all links from the page
5. Follows only internal links (same domain)
6. Tracks visited URLs to prevent infinite loops
7. Saves each page as a `.md` file preserving the directory structure
8. Repeats until all internal pages have been visited

## Output

Each downloaded page is saved as a Markdown file preserving the directory structure:
- `https://example.com/` → `index.md`
- `https://example.com/page1.html` → `page1.md`
- `https://example.com/docs/guide.html` → `docs/guide.md`
- `https://example.com/blog/2024/article.html` → `blog/2024/article.md`

## Architecture

The application is designed with testability in mind:

- **`WebScraperService`**: Core scraping logic with dependency injection support
- **`UrlHelper`**: Utilities for URL validation and manipulation
- **`FilePathGenerator`**: Converts URLs to file paths with proper directory structure
- **`Program.cs`**: CLI interface using Spectre.Console for beautiful output

## Testing

The project includes comprehensive unit tests covering:
- **Markdown conversion**: Verifies HTML elements (headings, lists, links, bold, italic, code blocks, etc.) are correctly converted to Markdown format
- **URL filtering**: Ensures only internal links are followed (external links ignored)
- **Visited URL tracking**: Prevents duplicate downloads
- **URL to file path conversion**: Validates proper path generation
- **Directory structure preservation**: Maintains folder hierarchy
- **Fragment identifier handling**: Properly handles URL fragments

Run tests with:
```bash
cd WebScraper.Tests
dotnet test
```

The test suite includes 40 tests across three test classes:
- `MarkdownConversionTests`: 13 tests for HTML to Markdown conversion
- `WebScraperServiceTests`: 9 tests for scraping logic and markdown output
- `FilePathGeneratorTests`: 10 tests for path generation
- `UrlHelperTests`: 8 tests for URL manipulation

## Requirements

- .NET 9.0 SDK or later
