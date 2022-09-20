using System.Text.Json.Serialization;

namespace Linkding.Client.Models;

public class BookmarkUpdatePayload : BookmarkBase
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
}