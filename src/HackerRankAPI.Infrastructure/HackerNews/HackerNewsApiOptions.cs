namespace HackerRankAPI.Infrastructure.HackerNews;

public sealed class HackerNewsApiOptions
{
    public const string SectionName = "HackerNewsApi";

    public string BaseUrl { get; init; } = "https://hacker-news.firebaseio.com/v0";

    public int BestStoryIdsCacheSeconds { get; init; } = 30;

    public int StoryCacheSeconds { get; init; } = 120;

    public int RequestTimeoutSeconds { get; init; } = 10;

    public int RetryCount { get; init; } = 3;

    public int RetryBaseDelayMilliseconds { get; init; } = 200;

    public int CircuitBreakerFailuresAllowedBeforeBreaking { get; init; } = 5;

    public int CircuitBreakerDurationSeconds { get; init; } = 30;
}