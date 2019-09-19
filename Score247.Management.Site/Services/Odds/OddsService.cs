using System;
using System.Threading.Tasks;
using Score247.Management.Site.Repositories.Matches;
using Score247.Management.Site.ViewModels.Matches;
using Score247Management.Core.Odds.Models;

namespace Score247.Management.Site.Services.Odds
{
    public interface IOddsService
    {
        Task<MatchOdds> GetMatchOdds(string matchId, int betTypeId, string language, string format);

        Task<OddsEventViewModel> CreateOddsViewModel(string matchId, TimeSpan offset);
    }

    public class OddsService : IOddsService
    {
        public async Task<OddsEventViewModel> CreateOddsViewModel(string matchId, TimeSpan offset)
        {
            var repository = new MatchRepository();
            var match = await repository.GetMatchById(matchId, offset);
            var viewModel = new OddsEventViewModel();
            viewModel.Match = match;
            viewModel.MatchOdds = await GetMatchOdds(matchId, 1, "en-US", "des");
            return viewModel;
        }

        public async Task<MatchOdds> GetMatchOdds(string matchId, int betTypeId, string language, string format)
        {
            // TODO: implement get match odds
            return new MatchOdds("", null);
        }
    }
}