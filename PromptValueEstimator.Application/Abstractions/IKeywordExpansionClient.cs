using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PromptValueEstimator.Application.Abstractions
{
    public interface IKeywordExpansionClient
    {
        /// <summary>
        /// Prompt’tan türeyen seed’ler için ilişkili anahtar kelimeleri döndürür.
        /// Dış API (Serpstat/DataForSEO) implementasyonu Infrastructure’da yapılacak.
        /// </summary>
        Task<IReadOnlyList<string>> GetRelatedKeywordsAsync(
            IEnumerable<string> seedPhrases,
            string languageCode,
            string geoTarget,
            int maxResults,
            CancellationToken ct);
    }
}
