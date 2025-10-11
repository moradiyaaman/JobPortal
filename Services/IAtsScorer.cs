using System.Threading.Tasks;
using JobPortal.Models;

namespace JobPortal.Services
{
    public sealed class AtsResult
    {
        public int Score { get; set; }
        public string[] MatchedKeywords { get; set; }
        public string[] MissingKeywords { get; set; }
        public string[] Suggestions { get; set; }
    }

    public sealed class AtsRankResult
    {
        public int Score { get; set; }
        public string[] MatchedKeywords { get; set; }
        public string[] MissingKeywords { get; set; }
    }

    public interface IAtsScorer
    {
        Task<AtsResult> ScoreResumeAsync(ApplicationUser user);
        Task<AtsRankResult> RankScoreAsync(ApplicationUser applicant, Job job, string coverLetterText);
    }
}
