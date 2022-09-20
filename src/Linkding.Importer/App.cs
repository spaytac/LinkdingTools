using AngleSharp.Html.Parser;
using Linkding.Client;
using Linkding.Client.Models;
using Linkding.Client.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Linkding.Importer;

public class App
{
    private readonly LinkdingSettings _settings;
    private readonly LinkdingService _service;
    private readonly ILogger<App> _logger;

    public App(IOptions<LinkdingSettings> settings, LinkdingService service, ILogger<App> logger)
    {
        _settings = settings.Value;
        _service = service;
        _logger = logger;
    }

    public async Task RunHandler(string[] args)
    {
        _logger.Log(LogLevel.Information, "Start 123456");

        if (args.Length != 1)
        {
            _logger.LogError("path to file needed!");
            throw new Exception("path to file needed!");
        }

        var path = args[0];

        var fileInfo = new FileInfo(path);
        if (!fileInfo.Exists)
        {
            _logger.LogError($"path to file not valid! provided path {path}");
            throw new Exception($"path to file not valid! provided path {path}");
        }

        if (fileInfo.Exists)
        {
            var linkdingBookmarks = await _service.GetAllBookmarksAsync();
            
            var htmlParser = new HtmlParser();
            var document = htmlParser.ParseDocument(File.ReadAllText(fileInfo.FullName));

            var parser = new Parser();
            // var tags = await parser.GetTagsAsync(document);
            var bookmarks = await parser.GetBookmarksAsync(document);

            var newBookmarks = new Dictionary<string, BookmarkCreatePayload>();
            var updateBookmarks = new Dictionary<string, BookmarkUpdatePayload>();
            foreach (var bookmark in bookmarks)
            {
                var linkdingBookmarkCollection = linkdingBookmarks.Where(x => x.Url.Equals(bookmark.Url));
                if (linkdingBookmarkCollection.Count() > 0)
                {
                    foreach (var linkdingBookmark in linkdingBookmarkCollection)
                    {
                        BookmarkUpdatePayload updatePayload = null;
                        if (!updateBookmarks.ContainsKey(bookmark.Url))
                        {
                            updateBookmarks.Add(bookmark.Url,
                                new BookmarkUpdatePayload() {Url = bookmark.Url, Id = linkdingBookmark.Id});
                        }

                        updatePayload = updateBookmarks[bookmark.Url];

                        if (string.IsNullOrEmpty(updatePayload.Title))
                        {
                            if (!string.IsNullOrEmpty(linkdingBookmark.Title))
                            {
                                updatePayload.Title = linkdingBookmark.Title;
                            }
                            else if (!string.IsNullOrEmpty(linkdingBookmark.WebsiteTitle))
                            {
                                updatePayload.Title = linkdingBookmark.WebsiteTitle;
                            }
                            else
                            {
                                updatePayload.Title = bookmark.Title;
                            }
                        }

                        if (string.IsNullOrEmpty(updatePayload.Description))
                        {
                            if (!string.IsNullOrEmpty(linkdingBookmark.Description))
                            {
                                updatePayload.Description = linkdingBookmark.Description;
                            }
                            else if (!string.IsNullOrEmpty(linkdingBookmark.WebsiteDescription))
                            {
                                updatePayload.Title = linkdingBookmark.WebsiteDescription;
                            }
                            else
                            {
                                updatePayload.Description = bookmark.Description;
                            }
                        }

                        if (updatePayload.TagNames.Count() == 0)
                        {
                            if (linkdingBookmark.TagNames?.Count() > 0)
                            {
                                updatePayload.TagNames = linkdingBookmark.TagNames;
                            }

                            updatePayload.TagNames = updatePayload.TagNames.Union(bookmark.TagNames);
                        }
                        else
                        {
                            updatePayload.TagNames = updatePayload.TagNames.Union(bookmark.TagNames);
                        }
                    }
                }
                else
                {
                    if (!newBookmarks.ContainsKey(bookmark.Url))
                    {
                        newBookmarks.Add(bookmark.Url, new BookmarkCreatePayload()
                        {
                            Title = bookmark.Title, Description = bookmark.Description, Url = bookmark.Url,
                            TagNames = bookmark.TagNames
                        });
                    }
                }
            }

            if (newBookmarks.Count() > 0)
            {
                var bla = newBookmarks.Values;
            }

            if (updateBookmarks.Count() > 0)
            {
                var bookmarkValues = updateBookmarks.Values;
                await _service.UpdateBookmarkCollectionAsync(bookmarkValues);
            }
        }
    }
}