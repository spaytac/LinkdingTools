using Linkding.Client;
using Linkding.Client.Models;
using Linkding.Client.Options;
using Linkding.Updater.Handler;
using LinkdingUpdater.Handler;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Linkding.Updater;

public class App
{
    private readonly LinkdingSettings _settings;
    private readonly LinkdingService _service;
    private readonly ILogger<App> _logger;

    public App(IOptions<LinkdingSettings> settings, LinkdingService service, ILogger<App> logger)
    {
        _service = service;
        _logger = logger;
        _settings = settings.Value;
    }

    public async Task RunHandler(string[] args)
    {
        _logger.LogInformation($"Starting updating bookmarks for {_settings.Url}");

        _logger.LogInformation("Collecting Handler");
        var handlers = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(s => s.GetTypes())
            .Where(p => typeof(ITaskHandler).IsAssignableFrom(p) && p.IsClass);

        var updatedBookmarksCount = 0;
        var updateBookmarks = new List<Bookmark>();
        var deleteBookmarks = new List<Bookmark>();
        if (handlers != null && handlers.Count() > 0)
        {
            var linkdingBookmarks = await _service.GetAllBookmarksAsync();
            if (linkdingBookmarks.Count() > 0)
            {
                
                _logger.LogInformation($"{linkdingBookmarks.Count()} bookmarks found in {_settings.Url}");
                
                foreach (var handler in handlers)
                {
                    ITaskHandler handlerInstance = null;
                    try
                    {
                        handlerInstance = (ITaskHandler) Activator.CreateInstance(handler);

                        foreach (var linkdingBookmark in linkdingBookmarks)
                        {
                            try
                            {
                                _logger.LogDebug($"Start executing {handlerInstance.Command}");
                                // var updateBookmark = updateBookmarks.FirstOrDefault(x => x.Id == linkdingBookmark.Id);
                                var existingBookmarkIndexInt =
                                    updateBookmarks.FindIndex(x => x.Id == linkdingBookmark.Id);

                                var bookmarkInstance = existingBookmarkIndexInt != -1 ? updateBookmarks[existingBookmarkIndexInt] : linkdingBookmark;

                                var result = await handlerInstance.ProcessAsync(bookmarkInstance, _logger);

                                if (result.HasError)
                                {
                                    _logger.LogWarning(result.ErrorMessage, handlerInstance.Command);
                                }
                                else
                                {
                                    if (result.PerformAction)
                                    {
                                        if (result.Action == LinkdingItemAction.Delete)
                                        {
                                            if (existingBookmarkIndexInt != -1)
                                            {
                                                updateBookmarks.RemoveAt(existingBookmarkIndexInt);
                                            }

                                            var bookmarkToDelete = deleteBookmarks.FirstOrDefault(x =>
                                                x.Url.ToLower() == result.Instance.Url.ToLower());
                                            if (bookmarkToDelete == null)
                                            {
                                                deleteBookmarks.Add(result.Instance);
                                            }
                                        }
                                        else
                                        {
                                            if (existingBookmarkIndexInt != -1)
                                            {
                                                updateBookmarks[existingBookmarkIndexInt] = result.Instance;
                                            }
                                            else
                                            {
                                                updateBookmarks.Add(result.Instance);
                                            }
                                        }
                                    }
                                }

                                _logger.LogDebug($"Finished {handlerInstance.Command}");
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e);
                                var message = $"... {e.Message}";

                                if (handlerInstance != null && !string.IsNullOrEmpty(handlerInstance.Command))
                                {
                                    message = $"Error while executing {handlerInstance.Command}! {message}";
                                }
                                else
                                {
                                    message = $"Error while executing handler! {message}";
                                }

                                _logger.LogError(message, "Calling Handler", e);
                                // throw;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        throw;
                    }
                }
            }
            else
            {
                _logger.LogInformation($"no bookmarks found in {_settings.Url}");
            }
            
            if (updateBookmarks.Count() > 0)
            {
                _logger.LogDebug($"Start updating bookmarks");
                await _service.UpdateBookmarkCollectionAsync(updateBookmarks);
                _logger.LogDebug($"Successfully updated bookmarks");
            }
        }

        _logger.LogInformation($"Finished updating bookmarks for {_settings.Url}");
    }
}