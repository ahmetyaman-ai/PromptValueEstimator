using MediatR;
using PromptValueEstimator.Application.Models;

namespace PromptValueEstimator.Application.Features.Estimator;

public sealed record EstimatePromptVolumeQuery(
    string PromptText,
    string LanguageCode = "en",
    string GeoTarget = "US",
    string Engine = "google",
    int MaxRelatedKeywords = 50,
    bool IncludeTrends = false,
    double SimilarityThreshold = 0.7,
    bool IntentFilter = true
) : IRequest<PromptEstimationResult>;


public sealed record EstimatePromptVolumeResult(
    string PromptText,
    int EstimatedMonthlyPromptVolume,
    double ConfidenceScore,
    string ConfidenceLabel,
    IReadOnlyList<string> ConfidenceReasons,
    IReadOnlyList<PromptSuggestion> RelatedHighVolumePrompts,
    DateTimeOffset LastUpdated
);
