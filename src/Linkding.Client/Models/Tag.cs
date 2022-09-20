using System.Text.Json.Serialization;

namespace Linkding.Client.Models;

public class Tag
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    [JsonPropertyName("name")]
    public string Name { get; set; }
    [JsonPropertyName("date_added")]
    public DateTime DateAdded { get; set; }
}