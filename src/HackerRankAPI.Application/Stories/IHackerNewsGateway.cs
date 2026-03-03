using HackerRankAPI.Domain.Stories;

namespace HackerRankAPI.Application.Stories;

public interface IHackerNewsGateway
{
    Task<IReadOnlyCollection<int>> GetBestStoryIdsAsync(CancellationToken cancellationToken = default);

    Task<Story?> GetStoryByIdAsync(int id, CancellationToken cancellationToken = default);
}