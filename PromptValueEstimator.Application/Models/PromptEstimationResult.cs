namespace PromptValueEstimator.Application.Models
{
    public class PromptEstimationResult
    {
        public string PromptText { get; set; }
        public List<PromptSuggestion> Suggestions { get; set; } = new();
    }
}
