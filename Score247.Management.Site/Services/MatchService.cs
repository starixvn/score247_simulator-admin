using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Score247.Management.Site.Models;
using Score247.Management.Site.Models.Events;
using Score247.Management.Site.Repositories.Leagues;
using Score247.Management.Site.Repositories.Matches;
using Score247.Management.Site.Repositories.Matches.Models;
using Score247.Management.Site.Repositories.Teams;
using Score247Management.Core._Shared.Enumerations;
using Score247Management.Core.Matches.Models;

namespace Score247.Management.Site.Services
{
    public interface IMatchService
    {
        Task CreateMatchForActiveLeagues(DateTime createDate, TimeSpan offset);

        Task<IEnumerable<Score247Management.Core.Matches.Models.MatchSummary>> GetLiveMatch(string language);

        Task CreateLiveMatches(CreateLiveMatchModel createLiveMatchModel);

        Task UpdateLiveMatch(string matchId);

        Task CloseLiveMatch(string matchId);

        Task DeleteMatchesByDate(CreateDeleteMatchesModel createDeleteMatchesRequest, TimeSpan offset);

        Task<IEnumerable<Match>> SearchMatchByDateAndLeague(DateTime dateSearch, string leagueId, TimeSpan offset);
    }

    public class MatchService : IMatchService
    {
        private readonly string DirectoryRoot = AppDomain.CurrentDomain.BaseDirectory + "/App_Data";
        private readonly IConfiguration configuration;
        private readonly ISimulatorService simulatorService;
        private readonly IQueryService queryService;
        private readonly ITriggerJobService triggerJobService;

        public MatchService(IConfiguration configuration, ISimulatorService simulatorService, IQueryService queryService, ITriggerJobService triggerJobService)
        {
            this.configuration = configuration;
            this.simulatorService = simulatorService;
            this.queryService = queryService;
            this.triggerJobService = triggerJobService;
        }

        public async Task CreateMatchForActiveLeagues(DateTime createDate, TimeSpan offset)
        {
            var ran = new Random();
            var leagueRepo = new LeagueRepository();
            var teamsRepository = new TeamRepository();
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
            var activeLeagues = await leagueRepo.GetActiveLeagues();
            int splitTeamPosition = (teams.Count - 1) / 2;

            List<MatchEntity> matches = new List<MatchEntity>();
            foreach (var league in activeLeagues)
            {
                var eventDate = GetEventDate(createDate, offset, false);

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

            await SendCreateMatchesRequest(matches, true);
        }

        public async Task<IEnumerable<Score247Management.Core.Matches.Models.MatchSummary>> GetLiveMatch(string language)
        {
            return await queryService.GetLiveMatches(language);
        }

        private static DateTime GetEventDate(DateTime dateTime, TimeSpan offset, bool dontRandomTime)
        {
            var ran = new Random();
            var hour = dontRandomTime ? dateTime.Hour : ran.Next(0, 23);
            var minute = dontRandomTime ? dateTime.Minute : ran.Next(0, 12) * 5;

            return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, hour, minute, 0, DateTimeKind.Utc).Subtract(offset);
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

        private Sport_Event_Status RandomLiveMatchEventStatus(string homeTeamId, string awayTeamId)
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
            eventStatus.clock = new Clock() { match_time = "75:00" };
            eventStatus.match_status = MatchStatus.SecondHaft.DisplayName;
            eventStatus.period = 2;
            eventStatus.status = "live";
            return eventStatus;
        }

        private async Task<int> SendCreateMatchesRequest(List<MatchEntity> allMatches, bool requiredTriggerInsertMatch)
        {
            var matchByDateUTCGroup = allMatches.GroupBy(d => d.Scheduled.Date);
            foreach (var matchByDate in matchByDateUTCGroup)
            {
                var createDate = matchByDate.Key;
                var matches = matchByDate.ToList();
                var content = new Models.Events.DailyScheduleDto
                {
                    generated_at = createDate,
                    schema = "Matches",
                    sport_events = matches
                };
                var jsonValue = JsonConvert.SerializeObject(content);

                var beginDateToday = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day, 0, 0, 0, DateTimeKind.Utc);
                var requestPath = configuration.GetValue<string>("Simulator:DailySchedule");

                if (createDate < beginDateToday)
                {
                    requestPath = configuration.GetValue<string>("Simulator:DailyResults");
                    var dailyResults = new List<Models.Events.Result>();

                    foreach (var match in matches)
                    {
                        var matchWithResult = new Models.Events.Result()
                        {
                            sport_event = match,
                            sport_event_status = RandomMatchEventStatus(match.Competitors.ElementAtOrDefault(0).Id, match.Competitors.ElementAtOrDefault(1).Id)
                        };

                        dailyResults.Add(matchWithResult);
                    }

                    var dailyResultRequest = new Models.Events.DailyResultDto
                    {
                        generated_at = createDate,
                        schema = "Matches",
                        results = dailyResults
                    };
                    jsonValue = JsonConvert.SerializeObject(dailyResultRequest);
                }

                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(configuration.GetValue<string>("Simulator:Url"));

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
                await triggerJobService.TriggerCreateMatchJobs();
            }

            return 1;
        }

        public async Task CreateLiveMatches(CreateLiveMatchModel createLiveMatchModel)
        {
            await simulatorService.SendCleanExistData();
            await CreateLiveMatches(createLiveMatchModel.PostCount, createLiveMatchModel.LiveCount, createLiveMatchModel.PreCount, createLiveMatchModel.LeagueLimit);
        }

        private async Task CreateLiveMatches(int postCount, int liveCount, int preCount, int leagueLimit)
        {
            try
            {
                var createDate = DateTime.UtcNow;
                var ran = new Random();
                var leagueRepo = new LeagueRepository();
                var teamsRepository = new TeamRepository();
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
                var activeLeagues = (await leagueRepo.GetActiveLeagues()).ToList();

                var dailyResults = new List<Result>();
                var matches = new List<MatchEntity>();

                var prematch = GenerateLivePreMatches(preCount, leagueLimit, activeLeagues, teams);
                dailyResults.AddRange(prematch);
                matches.AddRange(prematch.Select(x => x.sport_event).ToList());

                var postMatch = GenerateLivePostMatches(postCount, leagueLimit, activeLeagues, teams);
                dailyResults.AddRange(postMatch);
                matches.AddRange(postMatch.Select(x => x.sport_event).ToList());

                var liveMatch = GenerateLiveMatches(liveCount, leagueLimit, activeLeagues, teams);
                dailyResults.AddRange(liveMatch);
                matches.AddRange(liveMatch.Select(x => x.sport_event).ToList());

                matches = matches.Select(result =>
                {
                    result.Status = "not_started";
                    result.MatchStatus = MatchStatus.NotStarted.DisplayName;
                    return result;
                }).ToList();

                await SendDailySchedule(matches, createDate);
                await triggerJobService.TriggerCreateMatchJobs();
                await CheckIfMatchInserted(matches.LastOrDefault().Id);

                dailyResults = dailyResults.Select(result =>
                {
                    result.sport_event.Status = result.sport_event_status.status;
                    result.sport_event.MatchStatus = result.sport_event_status.match_status;

                    return result;
                }).ToList();
                var content = await ReadLiveMatchFromFile();
                if (content == null)
                {
                    content = new DailyResultDto
                    {
                        generated_at = createDate,
                        schema = "Matches",
                        results = dailyResults
                    };
                }
                else
                {
                    content.generated_at = DateTime.UtcNow;
                    var newLiveMatches = dailyResults.ToList();
                    newLiveMatches.AddRange(content.results);
                    content.results = newLiveMatches;
                }

                await SaveLiveMatchToFile(content);

                await simulatorService.SendLiveResult(content);
                await triggerJobService.TriggerFetchLiveMatch();
            }
            catch (Exception)
            {
            }
        }

        private async Task SaveLiveMatchToFile(DailyResultDto content)
        {
            var resultFolder = $"{DirectoryRoot}";
            System.IO.Directory.CreateDirectory(resultFolder);
            var path = $"{resultFolder}/live.json";
            var json = JsonConvert.SerializeObject(content);

            System.IO.Directory.CreateDirectory(resultFolder);
            System.IO.File.Delete(path);
            System.IO.File.WriteAllText(path, json);
        }

        private async Task<DailyResultDto> ReadLiveMatchFromFile()
        {
            try
            {
                var resultFolder = $"{DirectoryRoot}";
                System.IO.Directory.CreateDirectory(resultFolder);

                var path = $"{resultFolder}/live.json";
                var jsonData = await File.ReadAllTextAsync(path);
                var content = JsonConvert.DeserializeObject<DailyResultDto>(jsonData);

                return content;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private async Task CheckIfMatchInserted(string id)
        {
            var matchRepository = new MatchRepository();

            var match = await matchRepository.GetMatchById(id);

            while (match == null)
            {
                await Task.Delay(300);
                match = await matchRepository.GetMatchById(id);
            }
        }

        private List<Result> GenerateLivePostMatches(int subCount, int leagueLimit, List<Score247Management.Core.Leagues.Models.League> activeLeagues, List<Score247Management.Core.Teams.Models.Team> teams)
        {
            var ran = new Random();
            List<Result> result = new List<Result>();
            int matchPerLeague;
            int splitTeamPosition = (teams.Count - 1) / 2;

            for (int i = 0; i < subCount; i++)
            {
                var league = activeLeagues[ran.Next(0, activeLeagues.Count - 1)];
                matchPerLeague = ran.Next(1, (int)Math.Sqrt(subCount));

                for (int j = 0; j < matchPerLeague; j++)
                {
                    var eventDate = GetEventForLiveDate(DateTime.Now, ran.Next(1, 5));

                    var indexHome = ran.Next(0, splitTeamPosition);
                    var homeTeam = teams[indexHome];

                    var indexAway = ran.Next(splitTeamPosition + 1, teams.Count - 1);
                    var awaitTeam = teams[indexAway];
                    var matchEntity = new MatchEntity(
                        league,
                        homeTeam,
                        awaitTeam,
                        eventDate);
                    matchEntity.MatchStatus = MatchStatus.Ended.DisplayName;
                    matchEntity.Status = MatchStatus.Closed.DisplayName;
                    result.Add(new Result()
                    {
                        sport_event = matchEntity,
                        sport_event_status = RandomMatchEventStatus(matchEntity.Competitors.ElementAtOrDefault(0).Id, matchEntity.Competitors.ElementAtOrDefault(1).Id)
                    });
                }
            }

            return result;
        }

        private DateTime GetEventForLiveDate(DateTime eventDate, int minute)
        {
            return eventDate.AddMinutes(minute);
        }

        private List<Result> GenerateLivePreMatches(int subCount, int leagueLimit, List<Score247Management.Core.Leagues.Models.League> activeLeagues, List<Score247Management.Core.Teams.Models.Team> teams)
        {
            var ran = new Random();
            List<Result> result = new List<Result>();
            int matchPerLeague;
            int splitTeamPosition = (teams.Count - 1) / 2;

            for (int i = 0; i < subCount; i++)
            {
                var league = activeLeagues[ran.Next(0, activeLeagues.Count - 1)];
                matchPerLeague = ran.Next(1, (int)Math.Sqrt(subCount));

                for (int j = 0; j < matchPerLeague; j++)
                {
                    var eventDate = GetEventForLiveDate(DateTime.Now, ran.Next(-5, 1));

                    var indexHome = ran.Next(0, splitTeamPosition);
                    var homeTeam = teams[indexHome];

                    var indexAway = ran.Next(splitTeamPosition + 1, teams.Count - 1);
                    var awaitTeam = teams[indexAway];

                    var matchEntity = new MatchEntity(
                        league,
                        homeTeam,
                        awaitTeam,
                        eventDate);

                    matchEntity.MatchStatus = MatchStatus.NotStarted.DisplayName;
                    matchEntity.Status = MatchStatus.NotStarted.DisplayName;
                    result.Add(new Result()
                    {
                        sport_event = matchEntity,
                        sport_event_status = new Sport_Event_Status()
                        {
                            status = "not_started"
                        }
                    });
                }
            }

            return result;
        }

        private List<Result> GenerateLiveMatches(int subCount, int leagueLimit, List<Score247Management.Core.Leagues.Models.League> activeLeagues, List<Score247Management.Core.Teams.Models.Team> teams)
        {
            var ran = new Random();
            List<Result> results = new List<Result>();
            int matchPerLeague;
            int splitTeamPosition = (teams.Count - 1) / 2;

            for (int i = 0; i < subCount; i++)
            {
                var league = activeLeagues[ran.Next(0, activeLeagues.Count - 1)];
                matchPerLeague = ran.Next(1, (int)Math.Sqrt(subCount));

                for (int j = 0; j < matchPerLeague; j++)
                {
                    var eventDate = GetEventForLiveDate(DateTime.Now, 0);

                    var indexHome = ran.Next(0, splitTeamPosition);
                    var homeTeam = teams[indexHome];

                    var indexAway = ran.Next(splitTeamPosition + 1, teams.Count - 1);
                    var awaitTeam = teams[indexAway];

                    var matchEntity = new MatchEntity(
                        league,
                        homeTeam,
                        awaitTeam,
                        eventDate);

                    matchEntity.MatchStatus = MatchStatus.Live.DisplayName;
                    matchEntity.Status = MatchStatus.Live.DisplayName;
                    var result = new Result()
                    {
                        sport_event = matchEntity,
                        sport_event_status = RandomLiveMatchEventStatus(matchEntity.Competitors.ElementAtOrDefault(0).Id, matchEntity.Competitors.ElementAtOrDefault(1).Id)
                    };
                    results.Add(result);
                }
            }

            return results;
        }

        private async Task SendDailySchedule(IEnumerable<MatchEntity> matches, DateTime eventDate)
        {
            var content = new DailyScheduleDto
            {
                generated_at = eventDate,
                schema = "Matches",
                sport_events = matches
            };
            await simulatorService.SendCleanExistData();
            await simulatorService.SendDailySchedule(content);
        }

        public async Task CloseLiveMatch(string matchId)
        {
            var content = await ReadLiveMatchFromFile();
            if (content != null)
            {
                var newLiveMatches = content.results.Where(x => x.sport_event.Id != matchId);
                content.results = newLiveMatches;
                content.generated_at = DateTime.UtcNow;
            }
            else
            {
                content = new DailyResultDto()
                {
                    generated_at = DateTime.UtcNow,
                    results = new List<Result>(),
                    schema = "Matches"
                };
            }

            await SaveLiveMatchToFile(content);
            await simulatorService.SendLiveResult(content);
            await triggerJobService.TriggerFetchLiveMatch();
        }

        public async Task DeleteMatchesByDate(CreateDeleteMatchesModel createDeleteMatchesRequest, TimeSpan offset)
        {
            var matchRepository = new MatchRepository();
            var fromDate = new DateTime(createDeleteMatchesRequest.EventDate.Year, createDeleteMatchesRequest.EventDate.Month, createDeleteMatchesRequest.EventDate.Day, 0, 0, 0, DateTimeKind.Utc).Subtract(offset);
            var toDate = new DateTime(createDeleteMatchesRequest.EventDate.Year, createDeleteMatchesRequest.EventDate.Month, createDeleteMatchesRequest.EventDate.Day, 23, 59, 59, DateTimeKind.Utc).Subtract(offset);
            await matchRepository.DeleteMatchByDate(fromDate, toDate, createDeleteMatchesRequest.Count);
        }

        public async Task<IEnumerable<Match>> SearchMatchByDateAndLeague(DateTime dateSearch, string leagueId, TimeSpan offset)
        {
            var repository = new MatchRepository();
            var fromDate = new DateTime(dateSearch.Year, dateSearch.Month, dateSearch.Day, 0, 0, 0, DateTimeKind.Utc).Subtract(offset);
            var toDate = new DateTime(dateSearch.Year, dateSearch.Month, dateSearch.Day, 23, 59, 59, DateTimeKind.Utc).Subtract(offset);

            return await repository.GetMatchByDataRangeWithOffset(fromDate, toDate, leagueId, offset);
        }

        public async Task UpdateLiveMatch(string matchId)
        {
            var matchRepository = new MatchRepository();
            var match = await matchRepository.GetMatchById(matchId);
            if (match == null)
            {
                return;
            }

            var content = await ReadLiveMatchFromFile();
            if (content == null)
            {
                content = new DailyResultDto
                {
                    generated_at = DateTime.UtcNow,
                    schema = "Matches",
                    results = new List<Result>()
                };
            }
            var newLiveMatches = content.results.Where(x => x.sport_event.Id != match.Id).ToList();
            var liveMatch = BuildLiveMatchFromMatch(match);

            newLiveMatches.Add(liveMatch);
            content.results = newLiveMatches;

            await SaveLiveMatchToFile(content);
            await simulatorService.SendLiveResult(content);
            await triggerJobService.TriggerFetchLiveMatch();
        }

        private Result BuildLiveMatchFromMatch(Match match)
        {
            var result = new Result()
            {
                sport_event = new MatchEntity()
                {
                    Competitors = match.Teams,
                    Id = match.Id,
                    Scheduled = match.EventDate,
                    TournamentRound = match.LeagueRound,
                    Season = match.League.LeagueSeason,
                    Tournament = match.League,
                    Venue = match.Venue
                },
                sport_event_status = new Sport_Event_Status()
                {
                    status = "live",
                    match_status = match.MatchResult?.MatchStatus.DisplayName,
                    home_score = match.MatchResult.HomeScore,
                    away_score = match.MatchResult.AwayScore,
                    period = match.MatchResult.Period,
                    clock = new Clock()
                    {
                        match_time = match.LatestTimeline?.MatchClock
                    },
                    period_scores = match.MatchResult.MatchPeriods?.Select(period => ConvertMatchPeriodToPeriodScore(period)),
                    aggregate_away_score = match.MatchResult.AggregateAwayScore,
                    aggregate_home_score = match.MatchResult.AggregateHomeScore,
                    aggregate_winner_id = match.MatchResult.AggregateWinnerId,
                    winner_id = match.MatchResult.WinnerId
                }
            };

            return result;
        }

        private Period_Scores ConvertMatchPeriodToPeriodScore(MatchPeriod matchPeriod)
        {
            return new Period_Scores()
            {
                away_score = matchPeriod.AwayScore,
                home_score = matchPeriod.HomeScore,
                number = matchPeriod.Number,
                type = matchPeriod.PeriodType.DisplayName
            };
        }
    }
}