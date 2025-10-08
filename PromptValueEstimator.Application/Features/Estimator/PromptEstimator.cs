
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

// internal project references:
using PromptValueEstimator.Application.Abstractions;
using PromptValueEstimator.Application.Features.Estimator;
using PromptValueEstimator.Application.Models;
using PromptValueEstimator.Application.Services;


namespace PromptValueEstimator.Application.Features.Estimate
{
    public class PromptEstimator: IPromptEstimator
    {
        private readonly IKeywordSeedProvider _seeds;
        private readonly IKeywordRelationProvider _keywords;
        private readonly IKeywordVolumeProvider _volumes;

        public PromptEstimator(
            IKeywordSeedProvider seeds,
            IKeywordRelationProvider keywords,
            IKeywordVolumeProvider volumes)
        {
            _seeds = seeds;
            _keywords = keywords;
            _volumes = volumes;
        }

        public async Task<PromptEstimationResult> EstimateAsync(
            EstimatePromptVolumeQuery request,
            CancellationToken ct)
        {
            // 1) seed kelimeler
            var seeds = await _seeds.GetSeedKeywordsAsync(
                request.PromptText, request.LanguageCode, request.GeoTarget, ct);

            // 2) ilişkili kelimeler
            var related = await _keywords.GetRelatedKeywordsAsync(
                seeds, request.LanguageCode, request.GeoTarget, request.MaxRelatedKeywords, ct);

            // 3) adayların birleşimi
            var candidates = seeds
                .Concat(related ?? new List<string>())
                .Distinct(System.StringComparer.OrdinalIgnoreCase)
                .ToList();

            // 4) DIŞ VERİ: Serpstat API'den aylık arama hacmini çek
            var volumeMap = new Dictionary<string, int>();
            foreach (var c in candidates)
            {
                var vol = await _volumes.GetVolumeAsync(c, ct);
                volumeMap[c] = vol;
            }

            // 5) Puanlama: hacim varsa öncelikli, yoksa heuristic fallback
            var suggestions = new List<PromptSuggestion>();
            foreach (var c in candidates)
            {
                var sim = SimilarityScorer.Jaccard(request.PromptText, c);
                if (sim < request.SimilarityThreshold)
                    continue;

                var intent = IntentScorer.Score(c, request.IntentFilter);

                suggestions.Add(new PromptSuggestion
                {
                    Text = c,
                    Similarity = sim,
                    IntentScore = intent,
                    EstimatedVolume = volumeMap.TryGetValue(c, out var v) ? v : 0
                });
            }

            // 6) sıralama
            var ordered = suggestions
                .OrderByDescending(s => s.EstimatedVolume)
                .ThenByDescending(s => s.Similarity)
                .ThenByDescending(s => s.IntentScore)
                .Take(request.MaxRelatedKeywords)
                .ToList();

            return new PromptEstimationResult
            {
                PromptText = request.PromptText,
                Suggestions = ordered
            };
        }
    }
}
