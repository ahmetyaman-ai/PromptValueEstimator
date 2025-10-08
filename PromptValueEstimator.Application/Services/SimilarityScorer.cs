namespace PromptValueEstimator.Application.Services;

public static class SimilarityScorer
{
    // basit Jaccard (token set) benzerliği
    public static double Jaccard(string a, string b)
    {
        var sa = a.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToHashSet();
        var sb = b.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToHashSet();
        if (sa.Count == 0 || sb.Count == 0) return 0;
        var intersect = sa.Intersect(sb).Count();
        var union = sa.Union(sb).Count();
        return union == 0 ? 0 : (double)intersect / union;
    }
}
