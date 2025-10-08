using PromptValueEstimator.Application.Abstractions;

namespace PromptValueEstimator.Infrastructure.KeywordExpansion;

public sealed class StubKeywordExpansionClient : IKeywordExpansionClient
{
    public Task<IReadOnlyList<string>> GetRelatedKeywordsAsync(
        IEnumerable<string> seedPhrases,
        string languageCode,
        string geoTarget,
        int maxResults,
        CancellationToken ct)
    {
        // Şimdilik basit bir stub: hiçbir ek keyword dönmüyor (Serpstat eklenince değişecek)
        return Task.FromResult<IReadOnlyList<string>>(Array.Empty<string>());
    }
}
