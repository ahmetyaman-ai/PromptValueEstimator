using System.Text;
using System.Text.RegularExpressions;

namespace PromptValueEstimator.Application.Services;

public static class TextNormalizer
{
    private static readonly Regex MultiSpace = new(@"\s{2,}", RegexOptions.Compiled);

    public static string Normalize(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return string.Empty;
        var s = input.Trim().ToLowerInvariant();
        var sb = new StringBuilder(s.Length);
        foreach (var ch in s)
        {
            if (char.IsLetterOrDigit(ch) || char.IsWhiteSpace(ch)) sb.Append(ch);
        }
        s = MultiSpace.Replace(sb.ToString(), " ").Trim();
        return s;
    }

    public static IEnumerable<string> NGrams(string text, int min = 1, int max = 4)
    {
        var tokens = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        for (int n = min; n <= max; n++)
        {
            for (int i = 0; i + n <= tokens.Length; i++)
            {
                yield return string.Join(' ', tokens.AsSpan(i, n).ToArray());
            }
        }
    }
}
