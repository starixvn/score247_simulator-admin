using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Fanex.Data.Repository;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Score247.Management.Site.Criterias.Match;
using Score247.Management.Site.Helpers;
using Score247.Management.Site.Models;
using Score247.Management.Site.Models.Events;
using Score247.Management.Site.Repositories.Leagues;
using Score247.Management.Site.Repositories.Matches.Models;
using Score247.Management.Site.Repositories.Teams;
using Score247.Management.Site.Services.Matchs;
using Score247Management.Core.Matches.Models;

namespace Score247.Management.Site.Repositories.Matches
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

        public async Task<Match> GetMatchById(string id)
        {
            var criteria = new GetMatchByIdCriteria { Id = id, Language = "en-US" };
            var matches = await repository.FetchAsync<Match>(criteria);
            if (matches == null || !matches.Any())
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

        public async Task<int> CreateMatches(MatchCloneCriteria criteria)
        {
            var matches = await GenerateMatches(criteria);
            Func<Task> triggerTask = StartInsertMatchesJobs;

            return await SendCreateMatchesRequest(criteria, matches, triggerTask);
        }

        private async Task<int> SendCreateMatchesRequest(MatchCloneCriteria criteria, List<MatchEntity> matches, Func<Task> triggerTask)
        {
            var content = new Models.DailyScheduleDto
            {
                generated_at = criteria.EventDate,
                schema = "Matches",
                sport_events = matches
            };

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(Configuration.GetValue<string>("Simulator:Url"));
                var jsonValue = JsonConvert.SerializeObject(content);

                using (var response = await client.PostAsync(
                        string.Format(Configuration.GetValue<string>("Simulator:DailySchedule") + "?date={0}",
                        criteria.EventDate.ToString("yyyy-MM-dd")),
                        new StringContent(jsonValue, Encoding.UTF8,
                        "application/json")))
                {
                    if (!response.IsSuccessStatusCode)
                    {
                        return -1;
                    }
                }

                await triggerTask.Invoke();
            }

            return 1;
        }

        private async Task StartInsertMatchesJobs()
        {
            var triggerClient = new HttpClient
            {
                BaseAddress = new Uri(Configuration.GetValue<string>("Simulator:FetchUrl"))
            };

            var request = new HttpRequestMessage(HttpMethod.Post, "");

            var fetchContent = new FormUrlEncodedContent(new[]{
                            new KeyValuePair < string, string > ("jobs[]", "FetchPreMatch"),
                            new KeyValuePair < string, string > ("jobs[]", "FetchPostMatch"),
                            new KeyValuePair < string, string > ("jobs[]", "FetchLiveMatch")});

            request.Content = fetchContent;

            await triggerClient.SendAsync(request);
        }

        private static async Task<List<MatchEntity>> GenerateMatches(MatchCloneCriteria criteria)
        {
            var ran = new Random();
            var leaguesRepository = new LeagueRepository();
            var teamsRepository = new TeamRepository();

            var leagues = (await leaguesRepository.GetLeagues())
                .Where(x => !x.Name.Contains("+")
                            && !x.Name.Contains("-"))
                .ToList();

            var teams = (await teamsRepository.GetTeams())
                .Where(x => !x.Name.Contains("+")
                            && !x.Name.Contains("-")
                            && !x.Name.Contains("(PEN)")
                            && !x.Name.Contains("(ET)")
                            && !x.Name.Contains("(W)")
                            && !x.Name.Contains("(R)")
                            && !x.Name.Contains("Kick")
                            && !x.Name.Contains("Corner"))
                .ToList();

            List<MatchEntity> newMatches = new List<MatchEntity>();
            int matchPerLeague;
            int splitTeamPosition = (teams.Count - 1) / 2;

            for (int i = 0; i < criteria.LimitMatch; i += matchPerLeague)
            {
                var league = leagues[ran.Next(0, leagues.Count - 1)];
                matchPerLeague = ran.Next(0, 15);

                for (int j = 0; j < matchPerLeague; j++)
                {
                    var eventDate = GetEventDate(criteria.EventDate);

                    var indexHome = ran.Next(0, splitTeamPosition); 
                    var homeTeam = teams[indexHome];

                    var indexAway = ran.Next(splitTeamPosition + 1, teams.Count - 1);
                    var awaitTeam = teams[indexAway];

                    var matchEntity = new MatchEntity(
                        league,
                        homeTeam,
                        awaitTeam,
                        eventDate);

                    newMatches.Add(matchEntity);
                }
            }

            return newMatches;
        }

        private static DateTime GetEventDate(DateTime dateTime)
        {
            var ran = new Random();
            var hour = ran.Next(0, 23);
            var minute = ran.Next(0, 12) * 5;

            return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, hour, minute, 0, DateTimeKind.Utc);
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