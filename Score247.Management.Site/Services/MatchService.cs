using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Score247.Management.Site.Models;
using Score247.Management.Site.Models.DataMappers;
using Score247.Management.Site.Models.Events;
using Score247.Management.Site.Repositories.Leagues;
using Score247.Management.Site.Repositories.Matches;
using Score247.Management.Site.Repositories.Matches.Models;
using Score247.Management.Site.Repositories.Teams;
using Score247Management.Core._Shared.Enumerations;
using Score247Management.Core.Matches.Models;
using Score247Management.Core.Matches.Models.Soccer.Core.Matches.Models;
using Score247Management.Core.Statistics.Models;
using Score247Management.Core.Teams.Models;
using MatchSummary = Score247Management.Core.Matches.Models.MatchSummary;

namespace Score247.Management.Site.Services
{
    public interface IMatchService
    {
        Task CreateMatchForActiveLeagues(DateTime createDate, TimeSpan offset);

        Task<IEnumerable<MatchSummary>> GetLiveMatch(string language, TimeSpan offset);

        Task CreateLiveMatches(CreateLiveMatchModel createLiveMatchModel);

        Task UpdateLiveMatch(string matchId);

        Task CloseLiveMatch(string matchId);

        Task DeleteMatchesByDate(CreateDeleteMatchesModel createDeleteMatchesRequest, TimeSpan offset);

        Task<IEnumerable<MatchSummary>> SearchMatchByDateAndLeague(DateTime dateSearch, string leagueId, TimeSpan offset);

        Task CloseAllLiveMatches();

        Task<Result> GetSimulatorResult(string matchId);

        Task SendCustomLiveMatch(string matchId, Sport_Event_Status sportEventStatus);

        Task ApplyMatchStatistic(string matchId, IFormCollection collection);

        Task EnableDisableCoverage(string matchId);

        Task DeleteAllMatches();

        Task ApplyMatchScenarios(string matchId, IFormCollection collection);

        Task<LineUpsDto> GenerateLineups(string matchId, string homeFormation, string awayFormation);

        Task ApplyLineups(LineUpsDto lineUpsDto);
    }

    public class MatchService : IMatchService
    {
        private static readonly string DirectoryRoot = AppDomain.CurrentDomain.BaseDirectory + "/App_Data";
        private readonly IConfiguration configuration;
        private readonly ISimulatorService simulatorService;
        private readonly IQueryService queryService;
        private readonly ITriggerJobService triggerJobService;
        private readonly IScenarioService scenarioService;
        private readonly ITimelineService timelineService;
        private readonly ILineupService lineupService;

        public MatchService(
            IConfiguration configuration,
            ISimulatorService simulatorService,
            IQueryService queryService,
            ITriggerJobService triggerJobService,
            IScenarioService scenarioService,
            ITimelineService timelineService,
            ILineupService lineupService)
        {
            this.configuration = configuration;
            this.simulatorService = simulatorService;
            this.queryService = queryService;
            this.triggerJobService = triggerJobService;
            this.scenarioService = scenarioService;
            this.timelineService = timelineService;
            this.lineupService = lineupService;
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
            var splitTeamPosition = (teams.Count - 1) / 2;

            var matches = new List<MatchEntity>();
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

        public async Task<IEnumerable<Score247Management.Core.Matches.Models.MatchSummary>> GetLiveMatch(string language, TimeSpan offset)
        {
            var matches = await queryService.GetLiveMatches(language);
            var result = matches.Select(match =>
            {
                match.DisplayDate = match.EventDate.Add(offset).ToString("dd/MM/yyyy HH:mm:ss");
                return match;
            });

            return result;
        }

        private static DateTime GetEventDate(DateTime dateTime, TimeSpan offset, bool doNotRandomTime)
        {
            var ran = new Random();
            var hour = doNotRandomTime ? dateTime.Hour : ran.Next(0, 23);
            var minute = doNotRandomTime ? dateTime.Minute : ran.Next(0, 12) * 5;

            return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, hour, minute, 0, DateTimeKind.Utc).Subtract(offset);
        }

        private static Sport_Event_Status RandomMatchEventStatus(string homeTeamId, string awayTeamId)
        {
            var ran = new Random();
            var eventStatus = new Sport_Event_Status
            {
                period_scores = new List<Period_Scores>()
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

        private Sport_Event_Status RandomLiveMatchEventStatus()
        {
            var ran = new Random();
            var eventStatus = new Sport_Event_Status
            {
                period_scores = new List<Period_Scores>()
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
            var matchByDateUtcGroup = allMatches.GroupBy(d => d.Scheduled.Date);
            foreach (var matchByDate in matchByDateUtcGroup)
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
                            sport_event_status = RandomMatchEventStatus(match.Competitors.ElementAtOrDefault(0)?.Id, match.Competitors.ElementAtOrDefault(1)?.Id)
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
            await simulatorService.SyncLiveMatches();
            await CreateLiveMatches(createLiveMatchModel.PostCount, createLiveMatchModel.LiveCount, createLiveMatchModel.PreCount, createLiveMatchModel.LeagueLimit);
        }

        private async Task CreateLiveMatches(int postCount, int liveCount, int preCount, int leagueLimit)
        {
            try
            {
                var createDate = DateTime.UtcNow;
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

                var subActiveLeague = activeLeagues.GetRange(0, Math.Min(leagueLimit, activeLeagues.Count));

                var dailyResults = new List<Result>();
                var matches = new List<MatchEntity>();

                var preMatches = GenerateLivePreMatches(preCount, subActiveLeague, teams);
                dailyResults.AddRange(preMatches);
                matches.AddRange(preMatches.Select(x => x.sport_event).ToList());

                var postMatches = GenerateLivePostMatches(postCount, subActiveLeague, teams);
                dailyResults.AddRange(postMatches);
                matches.AddRange(postMatches.Select(x => x.sport_event).ToList());

                var liveMatches = GenerateLiveMatches(liveCount, subActiveLeague, teams);
                dailyResults.AddRange(liveMatches);
                matches.AddRange(liveMatches.Select(x => x.sport_event).ToList());

                matches = matches.Select(result =>
                {
                    result.Status = MatchStatus.NotStarted.DisplayName;
                    result.MatchStatus = null;
                    return result;
                }).ToList();

                await SendDailySchedule(matches, createDate);
                await UpdateMatchInitInfo(matches);
                await triggerJobService.TriggerCreateMatchJobs();
                await CheckIfMatchInserted(matches.LastOrDefault()?.Id);

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

                SaveLiveMatchToFile(content);

                await simulatorService.SendLiveResult(content);
                await triggerJobService.TriggerFetchLiveMatch();

                foreach (var postMatch in postMatches)
                {
                    await scenarioService.RandomScenario(postMatch.sport_event.Id, 0);

                    await UpdateLiveMatch(postMatch.sport_event.Id);
                }

                foreach (var liveMatch in liveMatches)
                {
                    await scenarioService.RandomScenario(liveMatch.sport_event.Id, 0, true);

                    await UpdateLiveMatch(liveMatch.sport_event.Id);
                }
            }
            catch (Exception ex)
            {
            }
        }

        private async Task UpdateMatchInitInfo(IEnumerable<MatchEntity> matches)
        {
            foreach (var match in matches)
            {
                var matchTimelineDto = new MatchTimelineDto
                {
                    generated_at = match.Scheduled,
                    schema = "http://schemas.sportradar.com/bsa/soccer/v1/json/endpoints/soccer/match_timeline.json",
                    sport_event = SportEventMapper.MapSportEvent(match),
                    sport_event_status = null,
                    timeline = null,
                    coverage_info = new CoverageInfoDto()
                };

                await simulatorService.SendMatchTimelines(matchTimelineDto);
            }
        }

        public static void SaveLiveMatchToFile(DailyResultDto content)
        {
            var resultFolder = $"{DirectoryRoot}";
            Directory.CreateDirectory(resultFolder);
            var path = $"{resultFolder}/live.json";
            var json = JsonConvert.SerializeObject(content);

            Directory.CreateDirectory(resultFolder);
            File.Delete(path);
            File.WriteAllText(path, json);
        }

        public static async Task<DailyResultDto> ReadLiveMatchFromFile()
        {
            try
            {
                var resultFolder = $"{DirectoryRoot}";
                Directory.CreateDirectory(resultFolder);

                var path = $"{resultFolder}/live.json";
                var jsonData = await File.ReadAllTextAsync(path);
                var content = JsonConvert.DeserializeObject<DailyResultDto>(jsonData);

                return content;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static async Task CheckIfMatchInserted(string id)
        {
            var matchRepository = new MatchRepository();

            var match = await matchRepository.GetMatchById(id);

            while (match == null)
            {
                await Task.Delay(300);
                match = await matchRepository.GetMatchById(id);
            }
        }

        private static List<Result> GenerateLivePostMatches(int subCount, IList<Score247Management.Core.Leagues.Models.League> activeLeagues, IList<Score247Management.Core.Teams.Models.Team> teams)
        {
            var ran = new Random();
            var result = new List<Result>();
            var splitTeamPosition = (teams.Count - 1) / 2;

            for (var i = 0; i < subCount; i++)
            {
                var league = activeLeagues[ran.Next(0, activeLeagues.Count - 1)];

                var eventDate = GetEventForLiveDate(DateTime.Now, ran.Next(-120, -90));

                var indexHome = ran.Next(0, splitTeamPosition);
                var homeTeam = teams[indexHome];

                var indexAway = ran.Next(splitTeamPosition + 1, teams.Count - 1);
                var awayTeam = teams[indexAway];
                var matchEntity = new MatchEntity(
                    league,
                    homeTeam,
                    awayTeam,
                    eventDate)
                { MatchStatus = MatchStatus.Ended.DisplayName, Status = MatchStatus.Closed.DisplayName };
                result.Add(new Result()
                {
                    sport_event = matchEntity,
                    sport_event_status = new Sport_Event_Status()
                    {
                        status = "not_started"
                    }
                });
            }

            return result;
        }

        private static DateTime GetEventForLiveDate(DateTime eventDate, int minute)
        {
            return eventDate.AddMinutes(minute);
        }

        private List<Result> GenerateLivePreMatches(int subCount, IList<Score247Management.Core.Leagues.Models.League> activeLeagues, IList<Score247Management.Core.Teams.Models.Team> teams)
        {
            var ran = new Random();
            List<Result> result = new List<Result>();
            int splitTeamPosition = (teams.Count - 1) / 2;

            for (int i = 0; i < subCount; i++)
            {
                var league = activeLeagues[ran.Next(0, activeLeagues.Count - 1)];
                var eventDate = GetEventForLiveDate(DateTime.Now, ran.Next(1, 16));

                var indexHome = ran.Next(0, splitTeamPosition);
                var homeTeam = teams[indexHome];

                var indexAway = ran.Next(splitTeamPosition + 1, teams.Count - 1);
                var awaitTeam = teams[indexAway];

                var matchEntity = new MatchEntity(
                    league,
                    homeTeam,
                    awaitTeam,
                    eventDate)
                { MatchStatus = null, Status = null };

                result.Add(new Result()
                {
                    sport_event = matchEntity,
                    sport_event_status = new Sport_Event_Status()
                    {
                        status = "not_started"
                    }
                });
            }

            return result;
        }

        private List<Result> GenerateLiveMatches(int subCount, IList<Score247Management.Core.Leagues.Models.League> activeLeagues, IList<Score247Management.Core.Teams.Models.Team> teams)
        {
            var ran = new Random();
            List<Result> results = new List<Result>();
            int splitTeamPosition = (teams.Count - 1) / 2;

            for (int i = 0; i < subCount; i++)
            {
                var league = activeLeagues[ran.Next(0, activeLeagues.Count - 1)];
                var eventDate = GetEventForLiveDate(DateTime.Now, 0);

                var indexHome = ran.Next(0, splitTeamPosition);
                var homeTeam = teams[indexHome];

                var indexAway = ran.Next(splitTeamPosition + 1, teams.Count - 1);
                var awayTeam = teams[indexAway];

                var matchEntity = new MatchEntity(
                    league,
                    homeTeam,
                    awayTeam,
                    eventDate)
                { MatchStatus = MatchStatus.Live.DisplayName, Status = MatchStatus.Live.DisplayName };

                var result = new Result()
                {
                    sport_event = matchEntity,
                    sport_event_status = RandomLiveMatchEventStatus()
                };
                results.Add(result);
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

            SaveLiveMatchToFile(content);
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

        public async Task<IEnumerable<MatchSummary>> SearchMatchByDateAndLeague(DateTime dateSearch, string leagueId, TimeSpan offset)
        {
            var fromDate = new DateTimeOffset(dateSearch.Year, dateSearch.Month, dateSearch.Day, 0, 0, 0, offset);
            var toDate = new DateTimeOffset(dateSearch.Year, dateSearch.Month, dateSearch.Day, 23, 59, 59, offset);

            var matches = await queryService.GetMatchesByDateRange(fromDate, toDate, "en-US", offset);

            if (leagueId == "all")
            {
                return matches;
            }

            return matches.Where(match => match.LeagueId == leagueId);
        }

        public async Task UpdateLiveMatch(string matchId)
        {
            await simulatorService.SyncLiveMatches();

            var matchRepository = new MatchRepository();
            var match = await matchRepository.GetMatchById(matchId);
            var latestTimelineLog = await matchRepository.GetLatestMatchTimelinesLog(matchId);

            if (match == null)
            {
                return;
            }

            var content = await ReadLiveMatchFromFile() ?? new DailyResultDto
            {
                generated_at = DateTime.UtcNow,
                schema = "Matches",
                results = new List<Result>()
            };
            var newLiveMatches = content.results.Where(x => x.sport_event.Id != match.Id).ToList();
            var liveMatch = BuildLiveMatchFromMatch(match, latestTimelineLog);

            newLiveMatches.Add(liveMatch);
            content.results = newLiveMatches;

            SaveLiveMatchToFile(content);
            await simulatorService.SendLiveResult(content);
            await triggerJobService.TriggerFetchLiveMatch();
            await triggerJobService.TriggerFetchLiveMatchTimeline();
        }

        private Result BuildLiveMatchFromMatch(Match match, MatchPushEventDto latestTimeline)
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
                sport_event_status = latestTimeline?.payload.sport_event_status
            };

            return result;
        }

        public static Period_Scores ConvertMatchPeriodToPeriodScore(MatchPeriod matchPeriod)
        {
            return new Period_Scores()
            {
                away_score = matchPeriod.AwayScore,
                home_score = matchPeriod.HomeScore,
                number = matchPeriod.Number,
                type = matchPeriod.PeriodType?.DisplayName
            };
        }

        public async Task CloseAllLiveMatches()
        {
            var content = new DailyResultDto()
            {
                generated_at = DateTime.UtcNow,
                schema = "Matches",
                results = new List<Result>()
            };

            SaveLiveMatchToFile(content);

            await simulatorService.SendLiveResult(content);
            await triggerJobService.TriggerFetchLiveMatch();
        }

        public async Task<Result> GetSimulatorResult(string matchId)
        {
            var dailyResult = await ReadLiveMatchFromFile();

            return dailyResult?.results?.FirstOrDefault(result => result.sport_event.Id == matchId);
        }

        public async Task SendCustomLiveMatch(string matchId, Sport_Event_Status sportEventStatus)
        {
            var result = await GetSimulatorResult(matchId);
            if (result == null)
            {
                return;
            }
            sportEventStatus.period_scores = result.sport_event_status.period_scores;

            result.sport_event_status = sportEventStatus;
            result.sport_event.Status = sportEventStatus.status;

            var content = await ReadLiveMatchFromFile() ?? new DailyResultDto
            {
                generated_at = DateTime.UtcNow,
                schema = "Matches",
                results = new List<Result>()
            };
            var newLiveMatches = content.results.Where(x => x.sport_event.Id != matchId).ToList();

            newLiveMatches.Add(result);
            content.results = newLiveMatches;

            SaveLiveMatchToFile(content);
            await simulatorService.SendLiveResult(content);
            await triggerJobService.TriggerFetchLiveMatch();
            await triggerJobService.TriggerFetchLiveMatchTimeline();
        }

        public async Task ApplyMatchStatistic(string matchId, IFormCollection collection)
        {
            var statistic = new Statistic()
            {
                MatchId = matchId,
                HomeStatistic = new TeamStatistic()
                {
                    RedCards = int.Parse(collection["HomeStatistic.RedCards"]),
                    Possession = int.Parse(collection["HomeStatistic.Possession"]),
                    CornerKicks = int.Parse(collection["HomeStatistic.CornerKicks"]),
                    Fouls = int.Parse(collection["HomeStatistic.Fouls"]),
                    FreeKicks = int.Parse(collection["HomeStatistic.FreeKicks"]),
                    GoalKicks = int.Parse(collection["HomeStatistic.GoalKicks"]),
                    Injuries = int.Parse(collection["HomeStatistic.Injuries"]),
                    Offsides = int.Parse(collection["HomeStatistic.Offsides"]),
                    ShotsBlocked = int.Parse(collection["HomeStatistic.ShotsBlocked"]),
                    ShotsOffTarget = int.Parse(collection["HomeStatistic.ShotsOffTarget"]),
                    ShotsOnTarget = int.Parse(collection["HomeStatistic.ShotsOnTarget"]),
                    ShotsSaved = int.Parse(collection["HomeStatistic.ShotsSaved"]),
                    ThrowIns = int.Parse(collection["HomeStatistic.ThrowIns"]),
                    YellowCards = int.Parse(collection["HomeStatistic.YellowCards"]),
                    YellowRedCards = int.Parse(collection["HomeStatistic.YellowRedCards"])
                },
                AwayStatistic = new TeamStatistic()
                {
                    RedCards = int.Parse(collection["AwayStatistic.RedCards"]),
                    Possession = int.Parse(collection["AwayStatistic.Possession"]),
                    CornerKicks = int.Parse(collection["AwayStatistic.CornerKicks"]),
                    Fouls = int.Parse(collection["AwayStatistic.Fouls"]),
                    FreeKicks = int.Parse(collection["AwayStatistic.FreeKicks"]),
                    GoalKicks = int.Parse(collection["AwayStatistic.GoalKicks"]),
                    Injuries = int.Parse(collection["AwayStatistic.Injuries"]),
                    Offsides = int.Parse(collection["AwayStatistic.Offsides"]),
                    ShotsBlocked = int.Parse(collection["AwayStatistic.ShotsBlocked"]),
                    ShotsOffTarget = int.Parse(collection["AwayStatistic.ShotsOffTarget"]),
                    ShotsOnTarget = int.Parse(collection["AwayStatistic.ShotsOnTarget"]),
                    ShotsSaved = int.Parse(collection["AwayStatistic.ShotsSaved"]),
                    ThrowIns = int.Parse(collection["AwayStatistic.ThrowIns"]),
                    YellowCards = int.Parse(collection["AwayStatistic.YellowCards"]),
                    YellowRedCards = int.Parse(collection["AwayStatistic.YellowRedCards"])
                }
            };
            await timelineService.ApplyStatistic(statistic);
            await UpdateLiveMatch(statistic.MatchId);

            await Task.Delay(2000);
        }

        public async Task EnableDisableCoverage(string matchId)
        {
            var matchRepo = new MatchRepository();
            var match = await matchRepo.GetMatchById(matchId);

            if (match.Coverage != null && match.Coverage.Live)
            {
                match.Coverage = new Coverage(false, "");
            }
            else
            {
                match.Coverage = new Coverage(true, "", true, true, true, true, true);
            }

            await timelineService.UpdatePreMatchInfo(match);
        }

        public async Task DeleteAllMatches()
        {
            await simulatorService.SendCleanExistData();
            await simulatorService.SendCleanTimelines();

            var matchRepository = new MatchRepository();
            await matchRepository.DeleteAllMatches();
        }

        public async Task ApplyMatchScenarios(string matchId, IFormCollection collection)
        {
            await scenarioService.ApplyScenario(matchId, collection);
        }

        public async Task<LineUpsDto> GenerateLineups(string matchId, string homeFormation, string awayFormation)
        {
            var matchRepository = new MatchRepository();
            var match = await matchRepository.GetMatchById(matchId);

            var lineUpDto = lineupService.BuildLineups(match, homeFormation, awayFormation);

            return lineUpDto;
        }

        public async Task ApplyLineups(LineUpsDto lineUpsDto)
        {
            await simulatorService.SendLineups(lineUpsDto);
        }
    }
}