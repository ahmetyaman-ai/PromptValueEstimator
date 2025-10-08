using PromptValueEstimator.Application.Abstractions;

namespace PromptValueEstimator.Infrastructure.KeywordExpansion;

public sealed class KeywordRelationProvider : IKeywordRelationProvider
{
    public Task<List<string>> GetRelatedKeywordsAsync(
        IEnumerable<string> seeds,
        string languageCode,
        string geoTarget,
        int maxRelatedKeywords,
        CancellationToken ct)
    {
        // Basit dummy örnek - ileride Serpstat veya LLM çağrısı eklenecek
        var related = seeds
            .Select(x => $"{x} tutorial")
            .Take(maxRelatedKeywords)
            .ToList();

        return Task.FromResult(related);
    }
}
