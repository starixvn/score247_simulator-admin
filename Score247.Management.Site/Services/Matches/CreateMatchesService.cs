using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Score247.Management.Site.Helpers;
using Score247.Management.Site.Models;
using Score247.Management.Site.Models.DataMappers;
using Score247.Management.Site.Models.Events;
using Score247.Management.Site.Repositories.Leagues;
using Score247.Management.Site.Repositories.Matches;
using Score247.Management.Site.Repositories.Teams;
using Score247Management.Core._Shared.Enumerations;
using Score247Management.Core.Enumerations;
using Score247Management.Core.Matches.Models;

namespace Score247.Management.Site.Services.Matches
{
    public interface ICreateMatchesService
    {
        Task CreateMatchForActiveLeagues(DateTime createDate, TimeSpan offset);

        Task CreateLiveMatches(CreateLiveMatchModel createLiveMatchModel);

        Task<bool> CreateMatchesByDate(CreateDeleteMatchesModel createDeleteMatchesRequest, TimeSpan offset);

        Task<string> CreateMatchWithForm(IFormCollection collection, TimeSpan offset);

        Task<int> CreateAllMatchesSite(List<CreateDeleteMatchesModel> createDeleteMatchesModels, TimeSpan offset);

        Task<string> AutoCreateMatch(DateTimeOffset dateTimeOffset);

        Task UpdateLiveMatch(string matchId);

        Sport_Event_Status RandomMatchEventStatus(string homeTeamId, string awayTeamId);
    }

    public class CreateMatchesService : ICreateMatchesService
    {
        private readonly ISimulatorService simulatorService;
        private readonly ITriggerJobService triggerJobService;
        private readonly IConfiguration configuration;
        private readonly ITimelineService timelineService;
        private readonly IMatchFileService matchFileService;
        private readonly IScenarioService scenarioService;

        public CreateMatchesService(ISimulatorService simulatorService,
            ITriggerJobService triggerJobService,
            IConfiguration configuration,
            ITimelineService timelineService,
            IMatchFileService matchFileService,
            IScenarioService scenarioService)
        {
            this.simulatorService = simulatorService;
            this.triggerJobService = triggerJobService;
            this.configuration = configuration;
            this.timelineService = timelineService;
            this.matchFileService = matchFileService;
            this.scenarioService = scenarioService;
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

        public async Task CreateLiveMatches(CreateLiveMatchModel createLiveMatchModel)
        {
            await simulatorService.SyncLiveMatches();
            await CreateLiveMatches(createLiveMatchModel.PostCount, createLiveMatchModel.LiveCount, createLiveMatchModel.PreCount, createLiveMatchModel.LeagueLimit);
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

            await triggerJobService.TriggerCreateMatchJobs();

            return 1;
        }

        public async Task<bool> CreateMatchesByDate(CreateDeleteMatchesModel createDeleteMatchesRequest, TimeSpan offset)
        {
            var createDate = new DateTime(createDeleteMatchesRequest.EventDate.Year, createDeleteMatchesRequest.EventDate.Month, createDeleteMatchesRequest.EventDate.Day, 0, 0, 0, DateTimeKind.Utc);

            await CreateMatches(createDate, createDeleteMatchesRequest.Count, offset, createDeleteMatchesRequest.LeagueLimit);

            return true;
        }

        public async Task<int> CreateMatches(DateTime createDate, int limitMatches, TimeSpan offset, int limitLeagues)
        {
            var matchRepository = new MatchRepository();
            var matches = await GenerateMatches(createDate, limitMatches, offset, limitLeagues);

            await SendCreateMatchesRequest(matches, true);
            return await matchRepository.WaitForMatchInserted(matches.LastOrDefault());
        }

        public async Task<string> CreateMatchWithForm(IFormCollection collection, TimeSpan offset)
        {
            var leagueRepo = new LeagueRepository();
            string leagueId = collection["Match.Tournament.Id"];
            var league = await leagueRepo.GetLeagueById(leagueId);

            var teamRepo = new TeamRepository();
            var teams = (await teamRepo.GetTeams()).ToList();
            var homeTeam = teams.FirstOrDefault(x => x.Id == collection["HomeTeam.Id"]);
            var awayTeam = teams.FirstOrDefault(x => x.Id == collection["AwayTeam.Id"]);

            var matchRepository = new MatchRepository();

            var newMatch = new MatchEntity
            {
                Id = UniqueGenerator.GetIdWithPattern("sr:match:"),
                Status = collection["Match.Status"],
                MatchStatus = collection["Match.Status"],
                Venue = new Score247Management.Core.Matches.Models.Venue
                {
                    Id = "sr:venue:" + collection["Match.Venue.Name"],
                    Name = collection["Match.Venue.Name"],
                    CountryName = collection["Match.Venue.CountryName"],
                    CountryCode = collection["Match.Venue.CountryCode"],
                    CityName = collection["Match.Venue.CityName"],
                    Capacity = int.Parse(collection["Match.Venue.Capacity"])
                },
                Referee = collection["Match.Referee"],
                Tournament = league
            };
            var competitors = new List<Score247Management.Core.Teams.Models.Team>();
            homeTeam.Statistic = new Score247Management.Core.Teams.Models.TeamStatistic();
            homeTeam.qualifier = "home";
            awayTeam.Statistic = new Score247Management.Core.Teams.Models.TeamStatistic();
            awayTeam.qualifier = "away";
            competitors.Add(homeTeam);
            competitors.Add(awayTeam);
            newMatch.Competitors = competitors;

            var dateTime = Convert.ToDateTime(collection["Match.Scheduled"]);
            var timeSpan = TimeSpan.Parse(collection["Match.ScheduledTime"]);
            newMatch.Scheduled = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds, DateTimeKind.Utc).Subtract(offset);
            newMatch.TournamentRound = new Score247Management.Core.Leagues.Models.LeagueRound
            {
                Type = Enumeration.FromDisplayName<LeagueRoundType>(collection["Match.TournamentRound.Type.Value"]),
                Name = collection["Match.TournamentRound.Name"],
                Number = int.Parse(collection["Match.TournamentRound.Number"]),
                Phase = collection["Match.TournamentRound.Phase"],
                Group = collection["Match.TournamentRound.Group"]
            };
            var beginningUtcToday = DateTimeHelper.GetBeginingOfDate(DateTime.UtcNow);
            if (newMatch.Scheduled < beginningUtcToday)
            {
                using (var response = await SendCreatePostMatch(configuration, newMatch))
                {
                    if (!response.IsSuccessStatusCode)
                    {
                        return "";
                    }
                }
            }
            else
            {
                using (var response = await SendCreatePreMatch(configuration, newMatch))
                {
                    if (!response.IsSuccessStatusCode)
                    {
                        return "";
                    }
                }
            }

            await timelineService.SendInitTimeline(newMatch);
            await triggerJobService.TriggerCreateMatchJobs();

            await matchRepository.WaitForMatchInserted(newMatch);

            return newMatch.Id;
        }

        public async Task<string> AutoCreateMatch(DateTimeOffset dateTimeOffset)
        {
            var matchRepository = new MatchRepository();
            var matches = await GenerateMatches(dateTimeOffset.DateTime, 1, dateTimeOffset.Offset, 1, true);

            await SendCreateMatchesRequest(matches, true);
            await matchRepository.WaitForMatchInserted(matches.LastOrDefault());

            return matches.First().Id;
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

            var content = await matchFileService.ReadLiveMatchFromFile() ?? new DailyResultDto
            {
                generated_at = DateTime.UtcNow,
                schema = "Matches",
                results = new List<Result>()
            };
            var newLiveMatches = content.results.Where(x => x.sport_event.Id != match.Id).ToList();
            var liveMatch = BuildLiveMatchFromMatch(match, latestTimelineLog);

            newLiveMatches.Add(liveMatch);
            content.results = newLiveMatches;

            matchFileService.SaveLiveMatchToFile(content);
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

        private async Task<int> SendCreateMatchesRequest(IEnumerable<MatchEntity> allMatches, bool requiredTriggerInsertMatch)
        {
            var matchByDateUtcGroup = allMatches.GroupBy(d => d.Scheduled.Date);
            foreach (var matchByDate in matchByDateUtcGroup)
            {
                var createDate = matchByDate.Key;
                var matches = matchByDate.ToList();
                var content = new DailyScheduleDto
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

                    var dailyResultRequest = new DailyResultDto
                    {
                        generated_at = createDate,
                        schema = "Matches",
                        results = dailyResults
                    };
                    jsonValue = JsonConvert.SerializeObject(dailyResultRequest);
                }
                else
                {
                    await UpdateMatchInitInfo(matches);
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

        private static async Task<HttpResponseMessage> SendCreatePostMatch(IConfiguration configuration, MatchEntity newMatch)
        {
            var dailyResults = new List<Result>();

            var matchWithResult = new Result()
            {
                sport_event = newMatch,
                sport_event_status = GenerateDefaultEventStatusFromMatchEntity(newMatch)
            };

            dailyResults.Add(matchWithResult);
            var dailyResultRequest = new DailyResultDto
            {
                generated_at = newMatch.Scheduled,
                schema = "Matches",
                results = dailyResults
            };
            var jsonValue = JsonConvert.SerializeObject(dailyResultRequest);
            var requestPath = configuration.GetValue<string>("Simulator:DailyResults");
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(configuration.GetValue<string>("Simulator:Url"));

                return await client.PostAsync(
                        string.Format(requestPath + "?date={0}",
                        newMatch.Scheduled.ToString("yyyy-MM-dd")),
                        new StringContent(jsonValue, Encoding.UTF8,
                        "application/json"));
            }
        }

        private static async Task<HttpResponseMessage> SendCreatePreMatch(IConfiguration configuration, MatchEntity newMatch)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(configuration.GetValue<string>("Simulator:Url"));
                var matchEntities = new List<MatchEntity> { newMatch };
                var content = new DailyScheduleDto
                {
                    generated_at = DateTime.UtcNow,
                    schema = "Matches",
                    sport_events = matchEntities
                };
                var jsonValue = JsonConvert.SerializeObject(content);

                var requestPath = configuration.GetValue<string>("Simulator:DailySchedule");

                return await client.PostAsync(string.Format(requestPath + "?date={0}", newMatch.Scheduled.ToString("yyyy-MM-dd")), new StringContent(jsonValue, Encoding.UTF8, "application/json"));
            }
        }

        private static Sport_Event_Status GenerateDefaultEventStatusFromMatchEntity(MatchEntity matchEntity)
        {
            var result = new Sport_Event_Status()
            {
                match_status = matchEntity.MatchStatus,
                aggregate_away_score = 0,
                aggregate_home_score = 0,
                aggregate_winner_id = null,
                away_score = 0,
                home_score = 0,
                clock = new Clock(),
                period = 1,
                period_scores = null,
                status = GetEventStatusFromMatchStatus(matchEntity.Status),
                winner_id = null
            };

            return result;
        }

        private static string GetEventStatusFromMatchStatus(string matchStatus)
        {
            if (matchStatus == MatchStatus.Ended.DisplayName
                || matchStatus == MatchStatus.EndedAfterPenalties.DisplayName
                || matchStatus == MatchStatus.EndedExtraTime.DisplayName
                || matchStatus == MatchStatus.Closed.DisplayName
                || matchStatus == MatchStatus.FullTime.DisplayName)
            {
                return MatchStatus.Closed.DisplayName;
            }
            else if (matchStatus == MatchStatus.NotStarted.DisplayName
                || matchStatus == MatchStatus.Postponed.DisplayName
                || matchStatus == MatchStatus.Cancelled.DisplayName
                || matchStatus == MatchStatus.StartDelayed.DisplayName
                || matchStatus == MatchStatus.Abandoned.DisplayName
                || matchStatus == MatchStatus.Pause.DisplayName)
            {
                return matchStatus;
            }

            return MatchStatus.Live.DisplayName;
        }

        public Sport_Event_Status RandomMatchEventStatus(string homeTeamId, string awayTeamId)
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

        private static async Task<List<MatchEntity>> GenerateMatches(DateTime createDate, int limitMatches, TimeSpan offset, int limitLeagues, bool doNotRandomTime = false)
        {
            var ran = new Random();
            var leaguesRepository = new LeagueRepository();
            var teamsRepository = new TeamRepository();

            var leagues = (await leaguesRepository.GetActiveLeagues())
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
                    var eventDate = GetEventDate(createDate, offset, doNotRandomTime);

                    var indexHome = ran.Next(0, splitTeamPosition);
                    var homeTeam = teams[indexHome];

                    var indexAway = ran.Next(splitTeamPosition + 1, teams.Count - 1);
                    var awayTeam = teams[indexAway];

                    var matchEntity = new MatchEntity(
                        league,
                        homeTeam,
                        awayTeam,
                        eventDate);

                    matches.Add(matchEntity);
                }
            }

            return matches.Take(limitMatches).ToList();
        }

        private static DateTime GetEventDate(DateTime dateTime, TimeSpan offset, bool doNotRandomTime)
        {
            var ran = new Random();
            var hour = doNotRandomTime ? dateTime.Hour : ran.Next(0, 23);
            var minute = doNotRandomTime ? dateTime.Minute : ran.Next(0, 12) * 5;

            return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, hour, minute, 0, DateTimeKind.Utc).Subtract(offset);
        }

        private async Task CreateLiveMatches(int postCount, int liveCount, int preCount, int leagueLimit)
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
            var content = await matchFileService.ReadLiveMatchFromFile();
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

            matchFileService.SaveLiveMatchToFile(content);

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
    }
}