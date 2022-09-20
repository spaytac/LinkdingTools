using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using Linkding.Client.Models;

namespace Linkding.Importer;

public class Parser
{
    public async Task<IEnumerable<Tag>> GetTagsAsync(IHtmlDocument document)
    {
        var tags = new List<Tag>();
        foreach (var element in document.QuerySelectorAll("DT H3"))
        {
            var dateAddedUnixEpochString = element.GetAttribute("ADD_DATE");
            DateTime dateAdded = dateAddedUnixEpochString.CreateDateTime();

            var tag = new Tag()
            {
                Name = element.TextContent
            };

            if (dateAdded != default)
            {
                tag.DateAdded = dateAdded;
                var year = dateAdded.GetYear();
                if (year != "1970")
                {
                    var yearTag = tags.FirstOrDefault(x => x.Name.Equals(year));
                    if (yearTag == null)
                    {
                        yearTag = new Tag() {Name = year, DateAdded = dateAdded};
                        tags.Add(yearTag);
                    }
                }
            }

            tags.Add(tag);
        }

        return tags;
    }

    public async Task<IEnumerable<Bookmark>> GetBookmarksAsync(IHtmlDocument document)
    {
        var bookmarks = new List<Bookmark>();

        foreach (var element in document.QuerySelectorAll("DT A"))
        {
            var href = element.GetAttribute("HREF");
            if (href.Equals("chrome://bookmarks/", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }


            var tags = new List<string>();

            var addDateTicksString = element.GetAttribute("ADD_DATE");
            var dateAdded = addDateTicksString.CreateDateTime();
            var year = dateAdded.GetYear();
            if (year != "1970")
            {
                tags.Add(year);
            }

            var nodeValues = element.TextContent.Split("|");
            var title = nodeValues[0];
            var description = "no description";
            if (nodeValues.Length > 1)
            {
                description = nodeValues[1];
            }

            var bookmarkark = new Bookmark()
            {
                Title = title, Description = description, Url = href, DateAdded = dateAdded, TagNames = tags, Unread = false, IsArchived = false
            };

            bookmarkark.TagNames = bookmarkark.TagNames.Concat(GetLinkTags(element));
            
            bookmarks.Add(bookmarkark);
        }

        return bookmarks;
    }

    private IEnumerable<string> GetLinkTags(IElement element)
    {
        var tags = new List<string>();
        var parentDl = element.Closest("DL");
        if (parentDl != null && parentDl.PreviousElementSibling != null)
        {
            var process = true;
            var node = parentDl.PreviousElementSibling;
            var persToolbarFolderString = node.GetAttribute("PERSONAL_TOOLBAR_FOLDER");
            if (!string.IsNullOrEmpty(persToolbarFolderString))
            {
                if (bool.TryParse(persToolbarFolderString, out var persToolbarFolder))
                {
                    process = !persToolbarFolder;
                }
            }

            if (process)
            {
                var title = node.TextContent;
                tags.Add(title);

                if (node.ParentElement != null)
                {
                    var otherTags = GetLinkTags(node.ParentElement);
                    if (otherTags.Count() > 0)
                    {
                        return tags.Concat(otherTags);
                    }
                }
            }
        }

        return tags;
    }
}