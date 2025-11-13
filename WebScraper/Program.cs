using Spectre.Console;
using WebScraper;

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

await ScrapeWebsiteWithProgress(url, outputDirectory);
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

static async Task ScrapeWebsiteWithProgress(string startUrl, string outputDirectory)
{
    var scraper = new WebScraperService();

    AnsiConsole.MarkupLine($"[green]Created output directory: {outputDirectory}[/]");

    await AnsiConsole.Progress()
        .StartAsync(async ctx =>
        {
            var task = ctx.AddTask("[green]Scraping website[/]");
            task.IsIndeterminate = true;

            var progress = new Progress<ScrapingProgress>(p =>
            {
                if (p.Status == "Downloading")
                {
                    AnsiConsole.MarkupLine($"[blue]Downloading:[/] {p.CurrentUrl}");
                }
                else if (p.Status == "Saved")
                {
                    AnsiConsole.MarkupLine($"[green]Saved:[/] {p.FilePath}");
                }
                else if (p.Status == "Error")
                {
                    AnsiConsole.MarkupLine($"[red]Error:[/] {p.ErrorMessage}");
                }

                task.Description = $"[green]Processed {p.PagesProcessed} pages, {p.PagesRemaining} remaining[/]";
            });

            var result = await scraper.ScrapeAsync(startUrl, outputDirectory, progress);

            if (!result.Success)
            {
                AnsiConsole.MarkupLine($"[red]✗ Scraping failed: {result.ErrorMessage}[/]");
                return;
            }

            task.StopTask();

            AnsiConsole.MarkupLine($"[green]✓ Scraping complete! Processed {result.PagesProcessed} pages.[/]");
            AnsiConsole.MarkupLine($"[green]Files saved to: {Path.GetFullPath(outputDirectory)}[/]");

            if (result.Errors.Count > 0)
            {
                AnsiConsole.MarkupLine($"[yellow]Warnings: {result.Errors.Count} errors occurred during scraping.[/]");
            }
        });
}
