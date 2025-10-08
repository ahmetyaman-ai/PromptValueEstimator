using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PromptValueEstimator.Application.Abstractions
{
    /// <summary>
    /// Provides related keywords for a given seed set.
    /// </summary>
    public interface IKeywordRelationProvider
    {
        Task<List<string>> GetRelatedKeywordsAsync(
            IEnumerable<string> seedKeywords,
            string languageCode,
            string geoTarget,
            int maxRelatedKeywords,
            CancellationToken ct);
    }
}
