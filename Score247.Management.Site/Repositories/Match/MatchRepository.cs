using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fanex.Data.Repository;
using Score247Management.Core.Matches.Models;
using Score247.Management.Site.Criterias.Match;
using Score247.Management.Site.Helpers;
using Score247.Management.Site.Models;
using Score247.Management.Site.Models.Events;
using Newtonsoft.Json;

namespace Score247.Management.Site.Repositories.Match
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
            var timelinesLogs = await repository.FetchAsync<TimelineLog>(new GetMatchTimelineLogCriteria() { MatchId = id });
            var timelines = await repository.FetchAsync<TimelineEvent>(new GetMatchTimelineCriteria() { MatchId = id });
            var result = timelines.Select(x => x.DeserialValue()).ToList();
            result.ForEach(x =>
            {
                var log = timelinesLogs.FirstOrDefault(y => y.TimelineId == x.Id);
                x.RequestLog = log?.Value;
            });

            return result;
        }

        public async Task<int> InsertTimelineLog(MatchPushEventDto eventDto)
        {
            var criteria = new InsertTimelineLogCriteria();
            criteria.Id = UniqueGenerator.GetIntId().ToString();
            criteria.MatchId = eventDto.metadata.sport_event_id;
            criteria.TimelineId = eventDto.payload.timeline.id;
            criteria.Value = JsonConvert.SerializeObject(eventDto);
            return await repository.ExecuteAsync(criteria);
        }

        public async Task<IEnumerable<Score247Management.Core.Matches.Models.Match>> GetMatchByDataRange(DateSearchModel dateSearchModel)
        {
            IEnumerable<Score247Management.Core.Matches.Models.Match> matches;
            if (dateSearchModel.SelectedLeagueId == "all")
            {
                var criteria = new GetMatchByDateRangeCriteria()
                {
                    SportId = 1,
                    FromDate = dateSearchModel.FromDate,
                    ToDate = dateSearchModel.ToDate,
                    Language = "en-US"
                };
                matches = await repository.FetchAsync<Score247Management.Core.Matches.Models.Match>(criteria);
            }
            else
            {
                var criteria = new GetMatchByDateRangeAndLeagueCriteria()
                {
                    SportId = 1,
                    FromDate = dateSearchModel.FromDate,
                    ToDate = dateSearchModel.ToDate,
                    Language = "en-US",
                    LeagueId = dateSearchModel.SelectedLeagueId
                };
                matches = await repository.FetchAsync<Score247Management.Core.Matches.Models.Match>(criteria);
            }

            var result = matches.Select(x => x.DeserialValue());

            return result;
        }
    }
}
