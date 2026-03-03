using System.Collections.Concurrent;

using Microsoft.Extensions.Options;

namespace HackerRankAPI.Application.Stories;

public sealed class BestStoriesService(
    IHackerNewsGateway hackerNewsGateway,
    IOptions<BestStoriesOptions> options) : IBestStoriesService
{
    private readonly IHackerNewsGateway _hackerNewsGateway = hackerNewsGateway;
    private readonly BestStoriesOptions _options = options.Value;

    public async Task<IReadOnlyCollection<BestStoryDto>> GetBestStoriesAsync(int count, CancellationToken cancellationToken = default)
    {
        if (count <= 0 || count > _options.MaxStoriesPerRequest)
        {
            throw new ArgumentOutOfRangeException(nameof(count),
                $"The value of '{nameof(count)}' must be between 1 and {_options.MaxStoriesPerRequest}.");
        }

        var bestStoryIds = await _hackerNewsGateway.GetBestStoryIdsAsync(cancellationToken);
        var selectedStoryIds = bestStoryIds.Take(count).ToArray();
        var stories = new ConcurrentBag<BestStoryDto>();

        await Parallel.ForEachAsync(
            selectedStoryIds,
            new ParallelOptions
            {
                CancellationToken = cancellationToken,
                MaxDegreeOfParallelism = _options.MaxConcurrentStoryRequests
            },
            async (id, token) =>
            {
                var story = await _hackerNewsGateway.GetStoryByIdAsync(id, token);
                if (story is null)
                {
                    return;
                }

                stories.Add(new BestStoryDto(
                    story.Title,
                    story.Uri,
                    story.PostedBy,
                    story.Time,
                    story.Score,
                    story.CommentCount));
            });

        return stories
            .OrderByDescending(story => story.Score)
            .ToArray();
    }
}