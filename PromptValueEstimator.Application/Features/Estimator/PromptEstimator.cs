

using PromptValueEstimator.Application.Abstractions;
using PromptValueEstimator.Application.Features.Estimator;
using PromptValueEstimator.Application.Services;

public sealed class PromptEstimator : IPromptEstimator
{
    private readonly IKeywordExpansionClient _keywords;
    private readonly IKeywordVolumeProvider _volumes;

    public PromptEstimator(IKeywordExpansionClient keywords, IKeywordVolumeProvider volumes)
    {
        _keywords = keywords;
        _volumes = volumes;
    }

    public async Task<EstimatePromptVolumeResult> EstimateAsync(EstimatePromptVolumeQuery request, CancellationToken ct)
    {
        var norm = TextNormalizer.Normalize(request.PromptText);

        // 1) seed ifadeleri
        var seeds = TextNormalizer.NGrams(norm, 1, 4)
            .Concat(new[] { $"explain {norm}", $"how to {norm}" })
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(request.MaxRelatedKeywords)
            .ToList();

        // 2) ilişkili anahtar kelimeler (Serpstat veya stub)
        var related = await _keywords.GetRelatedKeywordsAsync(
            seeds, request.LanguageCode, request.GeoTarget, request.MaxRelatedKeywords, ct);

        // 3) adayların birleşimi
        var candidates = seeds
            .Concat(related ?? Array.Empty<string>())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        // 4) DIŞ VERİ: aylık arama hacmi (Serpstat) — yoksa boş döner
        var volumeMap = await _volumes.GetMonthlySearchVolumeAsync(
            candidates, request.LanguageCode, request.GeoTarget, ct);

        // 5) puanlama (hacim varsa kullan, yoksa heuristic fallback)
        var suggestions = new List<PromptSuggestion>();
        foreach (var c in candidates)
        {
            var sim = SimilarityScorer.Jaccard(norm, TextNormalizer.Normalize(c));
            if (sim < request.SimilarityThreshold) continue;

            var intent = IntentScorer.Score(c, request.IntentFilter);

            // Dış hacim → öncelikli; yoksa heuristic fallback
            volumeMap.TryGetValue(c, out var volFromApi);
            var estVolume = volFromApi > 0
                ? volFromApi
                : (int)Math.Round(300 * sim * intent); // eski heuristics (düşürülmüş katsayı)

            suggestions.Add(new PromptSuggestion(c, estVolume, sim, intent));
        }

        // 6) toplam hacim
        var total = suggestions.Sum(s => s.EstimatedVolume);

        // 7) confidence
        var hasExternal = volumeMap.Count > 0;
        var highSimShare = suggestions.Count == 0 ? 0
            : suggestions.Count(s => s.Similarity >= 0.8) / (double)suggestions.Count;

        var confidence = 0.40 * highSimShare
                       + 0.25 * Math.Min(1.0, suggestions.Count / 20.0)
                       + 0.35 * (hasExternal ? 1.0 : 0.4);

        var label = confidence >= 0.75 ? "High" : confidence >= 0.5 ? "Medium" : "Low";

        return new EstimatePromptVolumeResult(
            PromptText: request.PromptText,
            EstimatedMonthlyPromptVolume: total,
            ConfidenceScore: Math.Round(confidence, 2),
            ConfidenceLabel: label,
            ConfidenceReasons: new[]
            {
                hasExternal ? "External volume data: Serpstat" : "No external volume (fallback heuristic)",
                $"HighSimShare={highSimShare:0.00}"
            },
            RelatedHighVolumePrompts: suggestions
                .OrderByDescending(s => s.EstimatedVolume)
                .Take(20)
                .ToList(),
            LastUpdated: DateTimeOffset.UtcNow
        );
    }
}
