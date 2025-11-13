using HtmlAgilityPack;
using ReverseMarkdown;
using Spectre.Console;

// Parse command line arguments
if (args.Length == 0 || args.Contains("--help") || args.Contains("-h"))
{
    ShowHelp();
    return 0;
}

string? url = null;
string outputDirectory = "output";

// Parse arguments
for (int i = 0; i < args.Length; i++)
{
    if (args[i] == "--output" || args[i] == "-o")
    {
        if (i + 1 < args.Length)
        {
            outputDirectory = args[i + 1];
            i++;
        }
    }
    else if (!args[i].StartsWith("-"))
    {
        url = args[i];
    }
}

if (string.IsNullOrEmpty(url))
{
    AnsiConsole.MarkupLine("[red]Error: URL is required![/]");
    ShowHelp();
    return 1;
}

await ScrapeWebsite(url, outputDirectory);
return 0;

static void ShowHelp()
{
    AnsiConsole.WriteLine();
    AnsiConsole.Write(new Rule("[bold blue]WebScraper[/]").RuleStyle("blue"));
    AnsiConsole.WriteLine();
    AnsiConsole.MarkupLine("[bold]Download and convert website to Markdown[/]");
    AnsiConsole.WriteLine();
    AnsiConsole.MarkupLine("[yellow]Usage:[/]");
    AnsiConsole.MarkupLine("  WebScraper [blue]<url>[/] [[options]]");
    AnsiConsole.WriteLine();
    AnsiConsole.MarkupLine("[yellow]Arguments:[/]");
    AnsiConsole.MarkupLine("  [blue]<url>[/]           The URL of the website to scrape");
    AnsiConsole.WriteLine();
    AnsiConsole.MarkupLine("[yellow]Options:[/]");
    AnsiConsole.MarkupLine("  [blue]-o, --output[/]    Output directory for markdown files (default: output)");
    AnsiConsole.MarkupLine("  [blue]-h, --help[/]      Show help information");
    AnsiConsole.WriteLine();
}

static async Task ScrapeWebsite(string startUrl, string outputDirectory)
{
    var visitedUrls = new HashSet<string>();
    var urlsToVisit = new Queue<string>();
    
    // Validate and normalize the start URL
    if (!Uri.TryCreate(startUrl, UriKind.Absolute, out var baseUri))
    {
        AnsiConsole.MarkupLine("[red]Invalid URL provided![/]");
        return;
    }

    // Create output directory if it doesn't exist
    if (!Directory.Exists(outputDirectory))
    {
        Directory.CreateDirectory(outputDirectory);
        AnsiConsole.MarkupLine($"[green]Created output directory: {outputDirectory}[/]");
    }

    urlsToVisit.Enqueue(startUrl);

    using var httpClient = new HttpClient();
    httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) WebScraper/1.0");

    var converter = new Converter();

    await AnsiConsole.Progress()
        .StartAsync(async ctx =>
        {
            var task = ctx.AddTask("[green]Scraping website[/]");
            task.IsIndeterminate = true;

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
                    AnsiConsole.MarkupLine($"[blue]Downloading:[/] {currentUrl}");

                    var html = await httpClient.GetStringAsync(currentUrl);
                    
                    // Convert HTML to Markdown
                    var markdown = converter.Convert(html);

                    // Generate a safe filename from the URL
                    var fileName = GenerateFileName(currentUrl, baseUri);
                    var filePath = Path.Combine(outputDirectory, fileName);

                    // Save the markdown file
                    await File.WriteAllTextAsync(filePath, markdown);
                    AnsiConsole.MarkupLine($"[green]Saved:[/] {fileName}");

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
                            if (Uri.TryCreate(baseUri, href, out var absoluteUri))
                            {
                                var absoluteUrl = absoluteUri.ToString();

                                // Only follow links that are on the same domain
                                if (absoluteUri.Host == baseUri.Host && !visitedUrls.Contains(absoluteUrl))
                                {
                                    // Remove fragment identifiers
                                    absoluteUrl = absoluteUrl.Split('#')[0];
                                    
                                    if (!visitedUrls.Contains(absoluteUrl) && !urlsToVisit.Contains(absoluteUrl))
                                    {
                                        urlsToVisit.Enqueue(absoluteUrl);
                                    }
                                }
                            }
                        }
                    }

                    task.Description = $"[green]Processed {visitedUrls.Count} pages, {urlsToVisit.Count} remaining[/]";
                }
                catch (HttpRequestException ex)
                {
                    AnsiConsole.MarkupLine($"[red]Error downloading {currentUrl}: {ex.Message}[/]");
                }
                catch (Exception ex)
                {
                    AnsiConsole.MarkupLine($"[red]Error processing {currentUrl}: {ex.Message}[/]");
                }
            }

            task.StopTask();
        });

    AnsiConsole.MarkupLine($"[green]✓ Scraping complete! Processed {visitedUrls.Count} pages.[/]");
    AnsiConsole.MarkupLine($"[green]Files saved to: {Path.GetFullPath(outputDirectory)}[/]");
}

static string GenerateFileName(string url, Uri baseUri)
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

    // Replace slashes with underscores and remove invalid filename characters
    var fileName = path.Replace("/", "_");
    
    // Remove query strings and fragments
    fileName = fileName.Split('?')[0].Split('#')[0];

    // Remove or replace invalid filename characters
    var invalidChars = Path.GetInvalidFileNameChars();
    foreach (var c in invalidChars)
    {
        fileName = fileName.Replace(c, '_');
    }

    // Ensure .md extension
    if (!fileName.EndsWith(".md", StringComparison.OrdinalIgnoreCase))
    {
        // If it has an HTML extension, replace it
        if (fileName.EndsWith(".html", StringComparison.OrdinalIgnoreCase) || 
            fileName.EndsWith(".htm", StringComparison.OrdinalIgnoreCase))
        {
            fileName = fileName.Substring(0, fileName.LastIndexOf('.'));
        }
        fileName += ".md";
    }

    return fileName;
}
