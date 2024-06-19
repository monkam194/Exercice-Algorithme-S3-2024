using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

public class WebCrawler
{
    private HashSet<string> visitedUrls = new HashSet<string>();
    private HashSet<string> foundEmails = new HashSet<string>();
    private int maxDepth;

    public WebCrawler(int maxDepth)
    {
        this.maxDepth = maxDepth;
    }

    public async Task<HashSet<string>> GetEmailsInPageAndChildPages(string filePath, int currentDepth)
    {
        if (currentDepth > maxDepth || visitedUrls.Contains(filePath))
        {
            return foundEmails;
        }

        visitedUrls.Add(filePath);
        string content;


        string absolutePath = Path.GetFullPath(filePath);
        content = await File.ReadAllTextAsync(absolutePath);

        var doc = new HtmlDocument();
        doc.LoadHtml(content);


        var emailNodes = doc.DocumentNode.SelectNodes("//a[starts-with(@href, 'mailto:')]");
        if (emailNodes != null)
        {
            foreach (var emailNode in emailNodes)
            {
                var href = emailNode.GetAttributeValue("href", string.Empty);
                var email = href.Substring(7); // Enlever 'mailto:'
                foundEmails.Add(email);
            }
        }

        var linkNodes = doc.DocumentNode.SelectNodes("//a[@href]");
        if (linkNodes != null)
        {
            foreach (var linkNode in linkNodes)
            {
                var link = linkNode.GetAttributeValue("href", string.Empty);
                if (!link.StartsWith("mailto:") && Uri.IsWellFormedUriString(link, UriKind.RelativeOrAbsolute))
                {
                    string nextFilePath;
                    if (Uri.IsWellFormedUriString(link, UriKind.Absolute))
                    {
                        nextFilePath = new Uri(link).LocalPath;
                    }
                    else
                    {
                        nextFilePath = Path.Combine(Path.GetDirectoryName(absolutePath), link);
                    }
                    await GetEmailsInPageAndChildPages(nextFilePath, currentDepth + 1);
                }
            }
        }

        return foundEmails;
    }

    public static async Task Main(string[] args)
    {
        var crawler = new WebCrawler(2);
        var emails = await crawler.GetEmailsInPageAndChildPages("../../../../TestHtml/index.html", 0);

        Console.WriteLine("les adresses mail trouvées: " + string.Join(", ", emails));
    }
}
