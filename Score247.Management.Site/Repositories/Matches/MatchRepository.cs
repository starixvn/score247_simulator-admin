using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fanex.Data.Repository;
using Newtonsoft.Json;
using Score247.Management.Site.Criterias.Match;
using Score247.Management.Site.Helpers;
using Score247.Management.Site.Models;
using Score247.Management.Site.Models.Events;
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
            var timelinesLogs = await repository.FetchAsync<TimelineLog>(new GetMatchTimelineLogCriteria() { Id = id });
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
            var timelinesLogs = await repository.FetchAsync<TimelineLog>(new GetMatchTimelineLogCriteria() { Id = id });
            var timelines = await repository.FetchAsync<TimelineEvent>(new GetMatchTimelineCriteria() { MatchId = id });
            var result = timelines.Select(x => x.DeserialValue(offset)).OrderBy(x => x.Time).ToList();
            result.ForEach(x =>
            {
                var log = timelinesLogs.FirstOrDefault(y => y.TimelineId == x.Id);
                x.RequestLog = log?.Value;
            });

            return result;
        }

        public async Task<IEnumerable<MatchPushEventDto>> GetMatchTimelinesLog(string id)
        {
            var timelinesLogs = await repository.FetchAsync<TimelineLog>(new GetMatchTimelineLogCriteria() { Id = id });

            var result = timelinesLogs.Select(timelineLog =>
                JsonConvert.DeserializeObject<MatchPushEventDto>(timelineLog.Value)).OrderBy(x => x.payload.timeline.time).ToList();

            return result;
        }

        public async Task<MatchPushEventDto> GetLatestMatchTimelinesLog(string id)
        {
            var timelinesLogs = await repository.FetchAsync<TimelineLog>(new GetMatchTimelineLogCriteria() { Id = id });

            var result = timelinesLogs.Select(timelineLog =>
                JsonConvert.DeserializeObject<MatchPushEventDto>(timelineLog.Value)).OrderByDescending(x => x.payload.timeline.time).FirstOrDefault();

            return result;
        }

        public async Task<int> InsertTimelineLog(MatchPushEventDto eventDto)
        {
            var criteria = new InsertTimelineLogCriteria
            {
                Id = eventDto.payload.timeline.id.ToString(),
                MatchId = eventDto.metadata.sport_event_id,
                TimelineId = eventDto.payload.timeline.id,
                RequestBody = JsonConvert.SerializeObject(eventDto),
                IsTimeline = true
            };
            return await repository.ExecuteAsync(criteria);
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
            var criteria = new DeleteMatchByDateCriteria() { FromDate = fromDate, ToDate = toDate, LimitCount = limitCount };
            return await repository.ExecuteAsync(criteria);
        }

        public async Task<int> DeleteMatchById(string id)
        {
            return await repository.ExecuteAsync(new DeleteMatchByIdCriteria() { Id = id });
        }
    }
}