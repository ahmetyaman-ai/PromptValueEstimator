namespace PromptValueEstimator.Infrastructure.Serpstat;

public sealed class SerpstatOptions
{
    public string BaseUrl { get; set; } = "https://api.serpstat.com";
    public string Token { get; set; } = "";
    public string DefaultEngine { get; set; } = "google";
    public string DefaultGeo { get; set; } = "US";
    public int TimeoutSeconds { get; set; } = 15;
}
