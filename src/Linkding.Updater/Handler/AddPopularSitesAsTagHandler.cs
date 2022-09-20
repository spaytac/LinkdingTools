using System.Text.RegularExpressions;
using Linkding.Client.Models;
using LinkdingUpdater.Handler;
using Microsoft.Extensions.Logging;

namespace Linkding.Updater.Handler;

public class AddPopularSitesAsTagHandler : ITaskHandler
{
    private record RegexExpressionGroups(string Expression, string Replace);

    private List<RegexExpressionGroups> Regexes = new()
    {
        new RegexExpressionGroups(@"https://(?:www\.)?(reddit)\.com(?:/r/)?([a-zA-Z0-9\-\+_]+)?(?:/.*)?", "$1,$2"),
        new RegexExpressionGroups(@"https://([a-zA-Z0-9]+)?[\.]?(microsoft)\.com(?:/.*)?", "$1,$2" ),
        new RegexExpressionGroups(@"https://(?:docs)\.(?:microsoft)\.com[/]?(?:[a-zA-Z0-9\-\+_]+)(?:/)?([a-zA-Z0-9\-\+_]+)?(?:/)?([a-zA-Z0-9\-\+_]+)?(?:/.*)?", "$1,$2" ),
        new RegexExpressionGroups(@"https://[[a-zA-Z0-9]+\.]?(youtube)\.com(?:/.*)?", "$1" ),
        new RegexExpressionGroups(@"https://[[a-zA-Z0-9]+\.]?(ebay)\.(com|de|fr)(?:/.*)?", "$1" ),
        new RegexExpressionGroups(@"https://[[a-zA-Z0-9]+\.]?(amazon)\.(com|de|fr)(?:/.*)?", "$1" ),
        new RegexExpressionGroups(@"https://([a-zA-Z0-9]+)?[\.]?(docker)\.com(?:/.*)?", "$1,$2" ),
        new RegexExpressionGroups(@"https://[[a-zA-Z0-9]+\.]?(xbox)\.com(?:/.*)?", "$1" ),
        new RegexExpressionGroups(@"https://([a-zA-Z0-9]+)?[\.]?(github)\.com[/]?([a-zA-Z0-9\-\+_]+)(?:/)?([a-zA-Z0-9\-\+_]+)?(?:/.*)?", "$1,$2,$3,$4" ),
        new RegexExpressionGroups(@"https://([a-zA-Z0-9]+)\.(github)\.io[/]?([a-zA-Z0-9\-\+_]+)(?:/)?([a-zA-Z0-9\-\+_]+)?(?:/.*)?", "$1,$2,$3")
    };

    private Dictionary<string, string> UrlTagMapping = new()
    {
        {"https://github.com/azure", "microsoft"},
        {"https://github.com/AzureAD", "microsoft"},
        {"https://github.com/dotnet-architecture", "microsoft"}
    };

    public string Command { get; } = "AddPopularSitesAsTag";
    public async Task<HandlerResult> ProcessAsync(Bookmark bookmark, ILogger logger)
    {
        var returnValue = new HandlerResult() {Instance = bookmark};
        Regex r = null;
        Match m = null;
        foreach (var regexEntry in Regexes)
        {
            try
            {
                r = new Regex(regexEntry.Expression, RegexOptions.IgnoreCase);
                m = r.Match(returnValue.Instance.Url);
                if (m.Success)
                {
                    var tagsCommaSeparated = r.Replace(returnValue.Instance.Url, regexEntry.Replace);
                    if (!string.IsNullOrEmpty(tagsCommaSeparated))
                    {
                        var tags = tagsCommaSeparated.Split(',');
                        foreach (var tag in tags)
                        {
                            if (!string.IsNullOrEmpty(tag) && !returnValue.Instance.TagNames.Contains(tag) &&
                                returnValue.Instance.TagNames.FirstOrDefault(x => x.ToLower() == tag.ToLower()) == null)
                            {
                                
                                returnValue.Instance.TagNames = returnValue.Instance.TagNames.Add(tag);
                                returnValue.PerformAction = true;
                                returnValue.Action = LinkdingItemAction.Update;
                            }
                        }
                    }
                }
            }
            finally
            {
                r = null;
                m = null;
            }
            
        }
        
        
        foreach (var urlKeyValue in UrlTagMapping)
        {
            if (returnValue.Instance.Url.ToLower().StartsWith(urlKeyValue.Key.ToLower()) && returnValue.Instance.TagNames.FirstOrDefault(x => x.ToLower() == urlKeyValue.Value.ToLower()) == null)
            {
                returnValue.Instance.TagNames = returnValue.Instance.TagNames.Add(urlKeyValue.Value);
                
                returnValue.PerformAction = true;
            }
        }


        return returnValue;
    }
}
