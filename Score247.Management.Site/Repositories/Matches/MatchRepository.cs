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
using Score247Management.Core._Shared.Enumerations;
using Score247Management.Core.Matches.Models;
using Result = Score247.Management.Site.Repositories.Matches.Models.Result;

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
            var jsonValue = JsonConvert.SerializeObject(content);

            var beginDateToday = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day, 0, 0, 0, DateTimeKind.Utc);
            var requestPath = Configuration.GetValue<string>("Simulator:DailySchedule");

            if (criteria.EventDate < beginDateToday)
            {
                requestPath = Configuration.GetValue<string>("Simulator:DailyResults");
                var dailyResults = new List<Result>();

                foreach (var match in matches)
                {
                    var matchWithResult = new Result()
                    {
                        sport_event = match,
                        sport_event_status = RandomMatchEventStatus(match.Competitors.ElementAtOrDefault(0).Id, match.Competitors.ElementAtOrDefault(1).Id)
                    };

                    dailyResults.Add(matchWithResult);
                }

                var dailyResultRequest = new Models.DailyResultDto
                {
                    generated_at = criteria.EventDate,
                    schema = "Matches",
                    results = dailyResults
                };
                jsonValue = JsonConvert.SerializeObject(dailyResultRequest);
            }

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(Configuration.GetValue<string>("Simulator:Url"));

                using (var response = await client.PostAsync(
                        string.Format(requestPath + "?date={0}",
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

        private Sport_Event_Status RandomMatchEventStatus(string homeTeamId, string awayTeamId)
        {
            var ran = new Random();
            var eventStatus = new Sport_Event_Status();
            eventStatus.period_scores = new List<Period_Scores>()
            {
                new Period_Scores()
                {
                    away_score = ran.Next(0, 2),
                    home_score = ran.Next(0, 2),
                    number = 1,
                    type = "regular_period"
                },
                new Period_Scores()
                {
                    away_score = ran.Next(0, 2),
                    home_score = ran.Next(0, 2),
                    number = 2,
                    type = "regular_period"
                }
            };
            eventStatus.home_score = eventStatus.period_scores.Sum(x => x.home_score);
            eventStatus.away_score = eventStatus.period_scores.Sum(x => x.away_score);
            eventStatus.aggregate_home_score = eventStatus.home_score;
            eventStatus.aggregate_away_score = eventStatus.away_score;
            eventStatus.winner_id = (eventStatus.home_score > eventStatus.away_score) ? homeTeamId : ((eventStatus.away_score > eventStatus.home_score) ? awayTeamId : null);
            eventStatus.aggregate_winner_id = eventStatus.winner_id;
            eventStatus.clock = new Clock() { match_time = "90:00" };
            eventStatus.match_status = MatchStatus.Ended.DisplayName;
            eventStatus.period = 2;
            eventStatus.status = "closed";
            return eventStatus;
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

            List<MatchEntity> matches = new List<MatchEntity>();
            int matchPerLeague;
            int splitTeamPosition = (teams.Count - 1) / 2;

            for (int i = 0; i < criteria.LimitMatch; i += matchPerLeague)
            {
                var league = leagues[ran.Next(0, leagues.Count - 1)];
                matchPerLeague = ran.Next(1, 15);

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

                    matches.Add(matchEntity);
                }
            }

            return matches.Take(criteria.LimitMatch).ToList();
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