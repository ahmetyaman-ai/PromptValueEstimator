namespace PromptValueEstimator.Application.Models
{
    /// <summary>
    /// Represents a single suggestion derived from a prompt, including similarity and estimated volume.
    /// </summary>
    public sealed record PromptSuggestion
    {
        public string Text { get; init; } = string.Empty;
        public double Similarity { get; init; }
        public double IntentScore { get; init; }
        public int EstimatedVolume { get; init; }
    }
}
