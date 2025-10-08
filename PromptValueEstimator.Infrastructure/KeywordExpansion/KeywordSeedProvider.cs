using PromptValueEstimator.Application.Abstractions;

namespace PromptValueEstimator.Infrastructure.KeywordExpansion;

public sealed class KeywordSeedProvider : IKeywordSeedProvider
{
    public Task<List<string>> GetSeedKeywordsAsync(
        string promptText,
        string languageCode,
        string geoTarget,
        CancellationToken ct)
    {
        // Basit dummy örnek - ileride OpenAI çağrısı eklenecek
        var list = new List<string>
        {
            promptText,
            $"{promptText} meaning",
            $"{promptText} example"
        };

        return Task.FromResult(list);
    }
}
