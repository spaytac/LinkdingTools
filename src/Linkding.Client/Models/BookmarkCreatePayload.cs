using System.Text.Json.Serialization;

namespace Linkding.Client.Models;

public class BookmarkCreatePayload : BookmarkBase
{
    [JsonPropertyName("is_archived")]
    public bool IsArchived { get; set; } = false;

    [JsonPropertyName("unread")] public bool Unread { get; set; } = false;
}