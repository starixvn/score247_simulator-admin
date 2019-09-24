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
using Score247.Management.Site.Services;
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

        public async Task<Match> GetMatchById(string id, TimeSpan offset)
        {
            var criteria = new GetMatchByIdCriteria { Id = id, Language = "en-US" };
            var matches = await repository.FetchAsync<Match>(criteria);
            if (matches == null || !matches.Any())
            {
                return null;
            }
            var result = matches.FirstOrDefault().DeserialValue(offset);

            return result;
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

        public async Task<int> CreateMatches(DateTime createDate, int limitMatches, TimeSpan offset, int limitLeagues)
        {
            var matches = await GenerateMatches(createDate, limitMatches, offset, limitLeagues);

            await SendCreateMatchesRequest(matches, true);
            return await CheckIfMatchInserted(matches.LastOrDefault());
        }

        public async Task<string> AutoCreateMatch(DateTimeOffset createDate)
        {
            var matches = await GenerateMatches(createDate.DateTime, 1, createDate.Offset, 1, true);

            await SendCreateMatchesRequest(matches, true);
            await CheckIfMatchInserted(matches.LastOrDefault());

            return matches.First().Id;
        }

        private async Task<int> CheckIfMatchInserted(MatchEntity match)
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

        public async Task<int> CreateLiveMatches(int count, int limitLeagues, string baseUrl, string eventPath)
        {
            var createDate = DateTime.UtcNow;
            var matches = await GenerateMatches(createDate, count, TimeSpan.Zero, limitLeagues);

            await SendCreateMatchesRequest(matches, true);
            return await SendTriggerEventForMatches(matches, baseUrl, eventPath);
        }

        public async Task<int> CreateAllMatchesSite(List<CreateDeleteMatchesModel> createDeleteMatchesModels, TimeSpan offset)
        {
            var matches = new List<MatchEntity>();
            foreach (var model in createDeleteMatchesModels)
            {
                var createDate = new DateTime(model.EventDate.Year, model.EventDate.Month, model.EventDate.Day, 0, 0, 0, DateTimeKind.Utc);
                var newMatches = await GenerateMatches(createDate, model.Count, offset, model.LeagueLimit);
                matches.AddRange(newMatches);
            }
            await SendCreateMatchesRequest(matches, false);
            await TriggerSimulatorService.StartInsertMatchesJobs(Configuration);

            return 1;
        }

        public async Task<int> DeleteLiveMatches(int count)
        {
            return await repository.ExecuteAsync(new DeleteLiveMatchCriteria() { LimitCount = count });
        }

        private async Task<int> SendTriggerEventForMatches(List<MatchEntity> matches, string baseUrl, string eventPath)
        {
            try
            {
                foreach (var match in matches)
                {
                    var existMatch = await GetMatchById(match.Id);
                    while (existMatch == null)
                    {
                        await Task.Delay(300);
                        existMatch = await GetMatchById(match.Id);
                    }
                    MatchPushEventDto eventStartMatch = MatchTimeEventCreator.CreateMatchStart(existMatch, existMatch.EventDate);
                    await AutoEventService.SendEvent(eventStartMatch, baseUrl, eventPath);
                }
            }
            catch (Exception)
            {
                return -1;
            }

            return 1;
        }

        private async Task<int> SendCreateMatchesRequest(List<MatchEntity> allMatches, bool requiredTriggerInsertMatch)
        {
            var matchByDateUTCGroup = allMatches.GroupBy(d => d.Scheduled.Date);
            foreach (var matchByDate in matchByDateUTCGroup)
            {
                var createDate = matchByDate.Key;
                var matches = matchByDate.ToList();
                var content = new Models.DailyScheduleDto
                {
                    generated_at = createDate,
                    schema = "Matches",
                    sport_events = matches
                };
                var jsonValue = JsonConvert.SerializeObject(content);

                var beginDateToday = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day, 0, 0, 0, DateTimeKind.Utc);
                var requestPath = Configuration.GetValue<string>("Simulator:DailySchedule");

                if (createDate < beginDateToday)
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
                        generated_at = createDate,
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
                            createDate.ToString("yyyy-MM-dd")),
                            new StringContent(jsonValue, Encoding.UTF8,
                            "application/json")))
                    {
                        if (!response.IsSuccessStatusCode)
                        {
                            return -1;
                        }
                    }
                }
            }

            if (requiredTriggerInsertMatch)
            {
                await TriggerSimulatorService.StartInsertMatchesJobs(Configuration);
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
            if (eventStatus.home_score > eventStatus.away_score)
            {
                eventStatus.winner_id = homeTeamId;
            }
            if (eventStatus.away_score > eventStatus.home_score)
            {
                eventStatus.winner_id = awayTeamId;
            }

            if (ran.Next(100) > 50)
            {
                eventStatus.aggregate_winner_id = eventStatus.winner_id;
                eventStatus.aggregate_home_score = eventStatus.home_score;
                eventStatus.aggregate_away_score = eventStatus.away_score;
            }
            eventStatus.clock = new Clock() { match_time = "90:00" };
            eventStatus.match_status = MatchStatus.Ended.DisplayName;
            eventStatus.period = 2;
            eventStatus.status = "closed";
            return eventStatus;
        }

        public async Task<int> DeleteAllMatches()
        {
            return await repository.ExecuteAsync(new DeleteAllMatchesCriteria());
        }

        private static async Task<List<MatchEntity>> GenerateMatches(DateTime createDate, int limitMatches, TimeSpan offset, int limitLeagues, bool dontRandomTime = false)
        {
            var ran = new Random();
            var leaguesRepository = new LeagueRepository();
            var teamsRepository = new TeamRepository();

            var leagues = (await leaguesRepository.GetLeagues())
                .Where(x => !x.Name.Contains("+")
                            && !x.Name.Contains("-"))
                .OrderBy(arg => Guid.NewGuid()).Take(limitLeagues)
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

            for (int i = 0; i < limitMatches; i += matchPerLeague)
            {
                var league = leagues[ran.Next(0, leagues.Count - 1)];
                matchPerLeague = ran.Next(1, (int)Math.Sqrt(limitMatches));

                for (int j = 0; j < matchPerLeague; j++)
                {
                    var eventDate = GetEventDate(createDate, offset, dontRandomTime);

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

            return matches.Take(limitMatches).ToList();
        }

        private static DateTime GetEventDate(DateTime dateTime, TimeSpan offset, bool dontRandomTime)
        {
            var ran = new Random();
            var hour = dontRandomTime ? dateTime.Hour : ran.Next(0, 23);
            var minute = dontRandomTime ? dateTime.Minute : ran.Next(0, 12) * 5;

            return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, hour, minute, 0, DateTimeKind.Utc).Subtract(offset);
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