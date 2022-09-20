using Linkding.Client.Models;
using LinkdingUpdater.Handler;
using Microsoft.Extensions.Logging;

namespace Linkding.Updater.Handler;

public class AddYearToBookmarkHandler : ITaskHandler
{
    public string Command { get; } = "AddYearToBookmark";

    public async Task<HandlerResult> ProcessAsync(Bookmark bookmark, ILogger logger)
    {
        var returnValue = new HandlerResult() {Instance = bookmark};

        var update = false;
        var createdYear = returnValue.Instance.DateAdded.GetYear();

        if (createdYear != "1970")
        {
            var tagName = returnValue.Instance.TagNames.FirstOrDefault(x => x.Equals(createdYear));
            if (tagName == null)
            {
                logger.LogInformation(
                    $"Detected bookmark ({returnValue.Instance.WebsiteTitle} - {returnValue.Instance.Id}) without year-tag ... Try to update");
                returnValue.Instance.TagNames = returnValue.Instance.TagNames.Add(createdYear);
                update = true;
            }
        }
        else
        {
            var wrongTagName = returnValue.Instance.TagNames.FirstOrDefault(x => x.Equals("1970"));
            if (wrongTagName != null)
            {
                logger.LogInformation(
                    $"Detected bookmark ({returnValue.Instance.WebsiteTitle} - {returnValue.Instance.Id}) with '1970' year-tag ... Try to update");
                returnValue.Instance.TagNames = returnValue.Instance.TagNames.Where(x => !x.Equals("1970")).Select(x => x);
                update = true;
            }
        }

        if (update)
        {
            logger.LogInformation($"Start updating bookmark {returnValue.Instance.WebsiteTitle} - {returnValue.Instance.Id}");
            returnValue.PerformAction = true;
            returnValue.Action = LinkdingItemAction.Update;
        }

        return returnValue;
    }
}