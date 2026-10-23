using Utils.SearchProviders.Models;

namespace Utils.SearchProviders.Contracts;

public interface IImageSearchService : ISearchService
{
    Task<IReadOnlyList<ImageCandidate>> SearchImageAsync(string query, CancellationToken cancellationToken);
}