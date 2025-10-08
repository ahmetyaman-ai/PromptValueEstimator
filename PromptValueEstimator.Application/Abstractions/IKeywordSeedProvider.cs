using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PromptValueEstimator.Application.Abstractions
{
    /// <summary>
    /// Generates initial seed keywords based on the given prompt.
    /// </summary>
    public interface IKeywordSeedProvider
    {
        Task<List<string>> GetSeedKeywordsAsync(
            string prompt,
            string languageCode,
            string geoTarget,
            CancellationToken ct);
    }
}
