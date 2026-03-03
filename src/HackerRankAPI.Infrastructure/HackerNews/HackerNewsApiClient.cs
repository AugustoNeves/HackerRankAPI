using System.Net.Http.Json;

using HackerRankAPI.Application.Stories;
using HackerRankAPI.Domain.Stories;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HackerRankAPI.Infrastructure.HackerNews;

internal sealed class HackerNewsApiClient(
    HttpClient httpClient,
    IMemoryCache cache,
    IOptions<HackerNewsApiOptions> options,
    ILogger<HackerNewsApiClient> logger) : IHackerNewsGateway
{
    private const string BestStoriesCacheKey = "hackernews:beststories";

    private readonly HttpClient _httpClient = httpClient;
    private readonly IMemoryCache _cache = cache;
    private readonly HackerNewsApiOptions _options = options.Value;
    private readonly ILogger<HackerNewsApiClient> _logger = logger;

    public async Task<IReadOnlyCollection<int>> GetBestStoryIdsAsync(CancellationToken cancellationToken = default)
    {
        return await _cache.GetOrCreateAsync(BestStoriesCacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(_options.BestStoryIdsCacheSeconds);
            var endpoint = $"{_options.BaseUrl.TrimEnd('/')}/beststories.json";
            var ids = await FetchAsync<int[]>(endpoint, cancellationToken);

            return (IReadOnlyCollection<int>)(ids ?? []);
        }) ?? [];
    }

    public async Task<Story?> GetStoryByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"hackernews:story:{id}";

        return await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(_options.StoryCacheSeconds);

            var endpoint = $"{_options.BaseUrl.TrimEnd('/')}/item/{id}.json";
            var response = await FetchAsync<HackerNewsStoryResponse>(endpoint, cancellationToken);

            if (response is null ||
                response.Deleted is true ||
                response.Dead is true ||
                !string.Equals(response.Type, "story", StringComparison.OrdinalIgnoreCase) ||
                string.IsNullOrWhiteSpace(response.Title) ||
                string.IsNullOrWhiteSpace(response.By))
            {
                return null;
            }

            return new Story(
                response.Id,
                response.Title,
                response.Url ?? string.Empty,
                response.By,
                DateTimeOffset.FromUnixTimeSeconds(response.Time),
                response.Score,
                response.Descendants ?? 0);
        });
    }

    private async Task<T?> FetchAsync<T>(string endpoint, CancellationToken cancellationToken)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<T>(endpoint, cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "Hacker News request failed for endpoint {Endpoint}", endpoint);
            return default;
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning(ex, "Hacker News request timed out for endpoint {Endpoint}", endpoint);
            return default;
        }
    }
}