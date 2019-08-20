using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fanex.Data.Repository;
using Score247Management.Core.Matches.Models;
using Score247Management.Criterias.Match;
using Score247Management.Helpers;

namespace Score247Management.Repositories.Match
{
    public class MatchRepository
    {
        private readonly IDynamicRepository repository;

        public MatchRepository()
        {
            repository = new DynamicRepository();
        }

        public async Task<IEnumerable<Score247Management.Core.Matches.Models.Match>> GetMatches()
        {
            var matches = await repository.FetchAsync<Score247Management.Core.Matches.Models.Match>(new GetMatchesCriteria());
            var result = matches.Select(x => x.DeserialValue());

            return result;
        }

        public async Task<Score247Management.Core.Matches.Models.Match> GetMatchById(string id)
        {
            var criteria = new GetMatchByIdCriteria { Id = id, Language = "en-US" };
            var matches = await repository.FetchAsync<Score247Management.Core.Matches.Models.Match>(criteria);
            var result = matches.FirstOrDefault().DeserialValue();

            return result;
        }
        public async Task<IEnumerable<TimelineEvent>> GetMatchTimelines(string id)
        {
            var timelines = await repository.FetchAsync<TimelineEvent>(new GetMatchTimelineCriteria() { MatchId = id});
            var result = timelines.Select(x => x.DeserialValue());

            return result;
        }
    }
}
