namespace HackerRankAPI.Application.Stories;

public sealed class BestStoriesOptions
{
    public const string SectionName = "BestStories";

    public int DefaultStoriesCount { get; init; } = 10;

    public int MaxStoriesPerRequest { get; init; } = 200;

    public int MaxConcurrentStoryRequests { get; init; } = 10;
}