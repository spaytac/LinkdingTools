using System.Text.Json.Serialization;

namespace Linkding.Client.Models;

public class TagCreatePayload
{
    [JsonPropertyName("name")]
    public string Name { get; set; }
}