using System.Text.Json.Serialization;

namespace Linkding.Client.Models;

public class BookmarkBase
{
    [JsonPropertyName("url")]
    public string Url { get; set; }
    [JsonPropertyName("title")]
    public string Title { get; set; }
    [JsonPropertyName("description")]
    public string Description { get; set; }
    [JsonPropertyName("tag_names")]
    public IEnumerable<string> TagNames { get; set; } = new List<string>();
}