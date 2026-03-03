namespace HackerRankAPI.Application.Stories;

public interface IBestStoriesService
{
    Task<IReadOnlyCollection<BestStoryDto>> GetBestStoriesAsync(int count, CancellationToken cancellationToken = default);
}