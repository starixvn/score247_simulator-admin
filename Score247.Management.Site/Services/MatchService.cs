using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Score247.Management.Site.Models.Events;
using Score247.Management.Site.Repositories.Leagues;
using Score247.Management.Site.Repositories.Matches.Models;
using Score247.Management.Site.Repositories.Teams;
using Score247Management.Core._Shared.Enumerations;
using Score247Management.Core.Matches.Models;

namespace Score247.Management.Site.Services
{
    public interface IMatchService
    {
        Task CreateMatchForActiveLeagues(DateTime createDate, TimeSpan offset);

        Task<IEnumerable<Match>> GetLiveMatch(string language);
    }

    public class MatchService : IMatchService
    {
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

        public async Task<IEnumerable<Match>> GetLiveMatch(string language)
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
    }
}