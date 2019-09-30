using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fanex.Data.Repository;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Score247.Management.Site.Criterias.Match;
using Score247.Management.Site.Helpers;
using Score247.Management.Site.Models.Events;
using Score247.Management.Site.Repositories.Leagues;
using Score247.Management.Site.Repositories.Matches.Models;
using Score247.Management.Site.Services.Matchs;
using Score247Management.Core.Matches.Models;

namespace Score247.Management.Site.Repositories.Matches
{
    public class MatchRepository
    {
        private readonly IDynamicRepository repository;

        public MatchRepository()
        {
            repository = new DynamicRepository();
        }

        public async Task<Match> GetMatchById(string id)
        {
            var criteria = new GetMatchByIdCriteria { Id = id, Language = "en-US" };
            var match = await repository.GetAsync<Match>(criteria);

            return match?.DeserialValue();
        }

        public async Task<Match> GetMatchById(string id, TimeSpan offset)
        {
            var criteria = new GetMatchByIdCriteria { Id = id, Language = "en-US" };
            var match = await repository.GetAsync<Match>(criteria);

            return match?.DeserialValue(offset);
        }

        public async Task<IEnumerable<TimelineEvent>> GetMatchTimelines(string id)
        {
            var timelinesLogs = await repository.FetchAsync<TimelineLog>(new GetMatchTimelineLogCriteria() { MatchId = id });
            var timelines = await repository.FetchAsync<TimelineEvent>(new GetMatchTimelineCriteria() { MatchId = id });
            var result = timelines.Select(x => x.DeserialValue()).OrderBy(x => x.Time).ToList();
            result.ForEach(x =>
            {
                var log = timelinesLogs.FirstOrDefault(y => y.TimelineId == x.Id);
                x.RequestLog = log?.Value;
            });

            return result;
        }

        public async Task<IEnumerable<TimelineEvent>> GetMatchTimelines(string id, TimeSpan offset)
        {
            var timelinesLogs = await repository.FetchAsync<TimelineLog>(new GetMatchTimelineLogCriteria() { MatchId = id });
            var timelines = await repository.FetchAsync<TimelineEvent>(new GetMatchTimelineCriteria() { MatchId = id });
            var result = timelines.Select(x => x.DeserialValue(offset)).ToList();
            result.ForEach(x =>
            {
                var log = timelinesLogs.FirstOrDefault(y => y.TimelineId == x.Id);
                x.RequestLog = log?.Value;
            });

            return result;
        }

        public async Task<int> InsertTimelineLog(MatchPushEventDto eventDto)
        {
            var criteria = new InsertTimelineLogCriteria
            {
                Id = UniqueGenerator.GetIntId().ToString(),
                MatchId = eventDto.metadata.sport_event_id,
                TimelineId = eventDto.payload.timeline.id,
                RequestBody = JsonConvert.SerializeObject(eventDto)
            };
            return await repository.ExecuteAsync(criteria);
        }

        public async Task<IEnumerable<Match>> GetMatchByDataRangeWithOffset(DateTime fromDate, DateTime toDate, string leagueId, TimeSpan offset)
        {
            IEnumerable<Match> matches;
            var criteria = new GetMatchByDateRangeCriteria()
            {
                SportId = 1,
                FromDate = fromDate,
                ToDate = toDate,
                Language = "en-US"
            };
            matches = await repository.FetchAsync<Match>(criteria);

            var result = matches.Select(x => x.DeserialValue(offset));

            if (leagueId != "all")
            {
                result = result.Where(x => x.League.Id == leagueId);
            }

            var leageRepository = new LeagueRepository();
            var leagues = await leageRepository.GetLeagues();

            result = result.Select(x =>
            {
                var fullLeague = leagues.FirstOrDefault(league => league.Id == x.League.Id);
                if (fullLeague != null)
                {
                    x.League = fullLeague;
                }

                return x;
            });

            return result;
        }

        public async Task<int> WaitForMatchInserted(MatchEntity match)
        {
            if (match == null)
            {
                return 1;
            }
            var insertedMatch = await GetMatchById(match.Id);
            while (insertedMatch == null)
            {
                await Task.Delay(300);
                insertedMatch = await GetMatchById(match.Id);
            }

            return 1;
        }

        public async Task<int> DeleteLiveMatches(int count)
        {
            return await repository.ExecuteAsync(new DeleteLiveMatchCriteria() { LimitCount = count });
        }
        public async Task<int> DeleteAllMatches()
        {
            return await repository.ExecuteAsync(new DeleteAllMatchesCriteria());
        }

        public async Task<int> DeleteMatchByDate(DateTime fromDate, DateTime toDate, int limitCount)
        {
            try
            {
                var criteria = new DeleteMatchByDateCriteria() { FromDate = fromDate, ToDate = toDate, LimitCount = limitCount };
                return await repository.ExecuteAsync(criteria);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<int> DeleteMatchById(string id)
        {
            return await repository.ExecuteAsync(new DeleteMatchByIdCriteria() { Id = id });
        }
    }
}