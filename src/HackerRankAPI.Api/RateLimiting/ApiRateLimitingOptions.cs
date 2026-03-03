namespace HackerRankAPI.Api.RateLimiting;

public sealed class ApiRateLimitingOptions
{
    public const string SectionName = "RateLimiting";

    public int PermitLimit { get; init; } = 60;

    public int WindowSeconds { get; init; } = 60;

    public int QueueLimit { get; init; } = 0;
}