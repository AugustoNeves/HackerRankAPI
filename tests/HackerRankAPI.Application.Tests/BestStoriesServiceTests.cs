using HackerRankAPI.Application.Stories;
using HackerRankAPI.Domain.Stories;

using Microsoft.Extensions.Options;

namespace HackerRankAPI.Application.Tests;

public sealed class BestStoriesServiceTests
{
    [Fact]
    public async Task GetBestStoriesAsync_WhenStoriesFound_ReturnsStoriesSortedByScoreDescending()
    {
        var gateway = new FakeGateway(
            bestIds: [1, 2, 3],
            stories: new Dictionary<int, Story?>
            {
                [1] = new Story(1, "A", "https://a", "u1", DateTimeOffset.UtcNow, 10, 1),
                [2] = new Story(2, "B", "https://b", "u2", DateTimeOffset.UtcNow, 100, 2),
                [3] = new Story(3, "C", "https://c", "u3", DateTimeOffset.UtcNow, 50, 3)
            });
        var options = Options.Create(new BestStoriesOptions
        {
            DefaultStoriesCount = 10,
            MaxStoriesPerRequest = 200,
            MaxConcurrentStoryRequests = 4
        });

        var service = new BestStoriesService(gateway, options);

        var result = await service.GetBestStoriesAsync(3);

        Assert.Collection(result,
            first => Assert.Equal(100, first.Score),
            second => Assert.Equal(50, second.Score),
            third => Assert.Equal(10, third.Score));
    }

    [Fact]
    public async Task GetBestStoriesAsync_WhenCountExceedsMax_ThrowsArgumentOutOfRangeException()
    {
        var gateway = new FakeGateway([], new Dictionary<int, Story?>());
        var options = Options.Create(new BestStoriesOptions
        {
            DefaultStoriesCount = 10,
            MaxStoriesPerRequest = 2,
            MaxConcurrentStoryRequests = 2
        });

        var service = new BestStoriesService(gateway, options);

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => service.GetBestStoriesAsync(3));
    }

    private sealed class FakeGateway(IReadOnlyCollection<int> bestIds, IReadOnlyDictionary<int, Story?> stories) : IHackerNewsGateway
    {
        public Task<IReadOnlyCollection<int>> GetBestStoryIdsAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(bestIds);

        public Task<Story?> GetStoryByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            stories.TryGetValue(id, out var story);
            return Task.FromResult(story);
        }
    }
}