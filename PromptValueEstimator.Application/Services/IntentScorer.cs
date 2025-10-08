namespace PromptValueEstimator.Application.Services;

public static class IntentScorer
{
    private static readonly string[] PromptVerbs =
    [
        "how to","explain","generate","write","create","convert","summarize",
        "examples of","example","translate","code","refactor","debug","compare","outline"
    ];

    public static double Score(string phrase, bool intentFilter)
    {
        if (!intentFilter) return 1.0;
        var p = phrase.ToLowerInvariant();
        // basit: başında/başlara yakın prompt-verb var mı?
        foreach (var v in PromptVerbs)
        {
            if (p.StartsWith(v) || p.Contains($" {v} ")) return 0.95;
        }
        // “nedir / nasıl” gibi TR kalıpları
        if (p.StartsWith("nedir") || p.StartsWith("nasıl") || p.Contains(" nasıl "))
            return 0.9;

        // nötr
        return 0.6;
    }
}
