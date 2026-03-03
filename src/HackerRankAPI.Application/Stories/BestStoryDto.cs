namespace HackerRankAPI.Application.Stories;

public sealed record BestStoryDto(
    string Title,
    string Uri,
    string PostedBy,
    DateTimeOffset Time,
    int Score,
    int CommentCount);