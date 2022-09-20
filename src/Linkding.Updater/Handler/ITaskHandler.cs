using Linkding.Client.Models;
using LinkdingUpdater.Handler;
using Microsoft.Extensions.Logging;

namespace Linkding.Updater.Handler;

public interface ITaskHandler
{
    string Command { get; }
    Task<HandlerResult> ProcessAsync(Bookmark bookmark, ILogger logger);
}