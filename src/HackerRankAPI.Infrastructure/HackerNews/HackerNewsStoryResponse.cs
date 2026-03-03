using System.Text.Json.Serialization;

namespace HackerRankAPI.Infrastructure.HackerNews;

internal sealed class HackerNewsStoryResponse
{
    [JsonPropertyName("id")]
    public int Id { get; init; }

    [JsonPropertyName("type")]
    public string? Type { get; init; }

    [JsonPropertyName("title")]
    public string? Title { get; init; }

    [JsonPropertyName("url")]
    public string? Url { get; init; }

    [JsonPropertyName("by")]
    public string? By { get; init; }

    [JsonPropertyName("time")]
    public long Time { get; init; }

    [JsonPropertyName("score")]
    public int Score { get; init; }

    [JsonPropertyName("descendants")]
    public int? Descendants { get; init; }

    [JsonPropertyName("deleted")]
    public bool? Deleted { get; init; }

    [JsonPropertyName("dead")]
    public bool? Dead { get; init; }
}