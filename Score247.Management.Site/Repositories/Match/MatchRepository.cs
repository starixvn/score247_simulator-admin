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
using Score247.Management.Site.Services.Matchs;
using Microsoft.Extensions.Configuration;
using System;
using Score247.Management.Site.Repositories.League;
using Score247.Management.Site.Repositories.Team;
using Score247.Management.Site.Repositories.Match.Models;
using System.Net.Http;
using System.Text;

namespace Score247.Management.Site.Repositories.Match
{
    public class MatchRepository
    {
        private readonly IDynamicRepository repository;
        private IConfiguration Configuration { get; set; }

        public MatchRepository()
        {
            repository = new DynamicRepository();
        }

        public MatchRepository(IConfiguration configuration)
        {
            repository = new DynamicRepository();
            Configuration = configuration;
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
            if(matches == null || matches.Count() == 0)
            {
                return null;
            }
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
            criteria.RequestBody = JsonConvert.SerializeObject(eventDto);
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

        public async Task<int> CloneMatches(MatchCloneCriteria criteria)
        {
            var ran = new Random();
            var leaguesRepository = new LeagueRepository();
            var leagues = (await leaguesRepository.GetLeagues()).ToList();
            var teamsRepository = new TeamRepository();
            var teams = (await teamsRepository.GetTeams()).ToList();
            List<NewMatch> newMatches = new List<NewMatch>();
            for (int i = 0; i < criteria.LimitMatch; i++)
            {
                var newMatch = new NewMatch(leagues[ran.Next(0, leagues.Count - 1)],
                    teams[ran.Next(0, teams.Count - 1)],
                    teams[ran.Next(0, teams.Count - 1)],
                    new DateTime(criteria.EventDate.Year, criteria.EventDate.Month, criteria.EventDate.Day, criteria.EventDate.Hour, criteria.EventDate.Minute, criteria.EventDate.Second, DateTimeKind.Utc));
                newMatch.Id = UniqueGenerator.GetIdWithPattern("sr:match:");
                newMatches.Add(newMatch);
            }

            var content = new Repositories.Match.Models.DailyScheduleDto();
            content.GeneratedAt = DateTime.UtcNow;
            content.Schema = "Matches";
            content.sport_events = newMatches;

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(Configuration.GetValue<string>("Simulator:Url"));
                var jsonValue = JsonConvert.SerializeObject(content);

                var response = await client.PostAsync(string.Format(Configuration.GetValue<string>("Simulator:DailySchedule") + "?date={0}", criteria.EventDate.ToString("yyyy-MM-dd")), new StringContent(jsonValue, Encoding.UTF8, "application/json"));
                if (!response.IsSuccessStatusCode)
                {
                    return -1;
                }

                var triggerClient = new HttpClient();
                triggerClient.BaseAddress = new Uri(Configuration.GetValue<string>("Simulator:FetchUrl"));
                var request = new HttpRequestMessage(HttpMethod.Post, "");
                FormUrlEncodedContent fetchContent = new FormUrlEncodedContent(
                    new[]{
                            new KeyValuePair < string, string > ("jobs[]", "FetchPreMatch"),
                            new KeyValuePair < string, string > ("jobs[]", "FetchPostMatch"),
                            new KeyValuePair < string, string > ("jobs[]", "FetchLiveMatch")
                });
                request.Content = fetchContent;
                await triggerClient.SendAsync(request);
            }
            return 1;
            //return await repository.ExecuteAsync(criteria);
        }

        public async Task CloneLiveMatches(GetCloneLiveMatchesCriteria criteria, string baseUrl, string eventPath)
        {

            var newLiveMatches = await repository.FetchAsync<Score247Management.Core.Matches.Models.Match>(criteria);
            foreach (var match in newLiveMatches)
            {
                var transMatch = match.DeserialValue();
                var insertCriteria = new InsertMatchCriteria()
                {
                    Id = transMatch.Id,
                    MatchValue = transMatch.Value,
                    SportId = "1",
                    MatchLanguage = "en-US",
                    EventDate = transMatch.EventDate.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:sszzz"),
                    LeagueId = transMatch.League.Id,
                    Region = transMatch.Region
                };
                await repository.ExecuteAsync(insertCriteria);
                var eventStartMatch = MatchTimeEventCreator.CreateMatchStart(transMatch, transMatch.EventDate);
                await AutoEventService.SendEvent(eventStartMatch, baseUrl, eventPath);
            }

        }
        
        public async Task<int> DeleteMatchByDate(DateTime dateTime)
        {
            var criteria = new DeleteMatchByDateCriteria() { DeleteDate = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, 0, 0, 0, DateTimeKind.Utc) };
            return await repository.ExecuteAsync(criteria);
        }
    }
}
