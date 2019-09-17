using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Score247.Management.Site.Helpers;
using Score247.Management.Site.Models;
using Score247.Management.Site.Models.Events;
using Score247.Management.Site.Repositories.Leagues;
using Score247.Management.Site.Repositories.Matches;
using Score247.Management.Site.Repositories.Matches.Models;
using Score247.Management.Site.Repositories.Teams;
using Score247Management.Core._Shared.Enumerations;
using Score247Management.Core.Teams.Models;

namespace Score247.Management.Site.Services.Matchs
{
    public interface CreateMatchesService
    {
        Task<bool> CreateLiveMatches(CreateDeleteMatchesModel createDeleteMatchesRequest, IConfiguration configuration);

        Task<bool> CreateMatchesByDate(CreateDeleteMatchesModel createDeleteMatchesRequest, TimeSpan offset, IConfiguration configuration);

        Task<string> CreateMatcheWithForm(IFormCollection collection, TimeSpan offset, IConfiguration configuration);

        Task<string> AutoCreateMatch(DateTimeOffset? dateTimeOffset, IConfiguration configuration);
    }

    public class CreateMatchesServiceImpl : CreateMatchesService
    {
        public async Task<bool> CreateLiveMatches(CreateDeleteMatchesModel createDeleteMatchesRequest, IConfiguration configuration)
        {
            var matchRepository = new MatchRepository(configuration);
            await SimulatorService.SendCleanExistData(configuration);
            await matchRepository.CreateLiveMatches(createDeleteMatchesRequest.Count, createDeleteMatchesRequest.LeagueLimit, configuration.GetValue<string>("Simulator:Url"), configuration.GetValue<string>("Simulator:PushEvent"));

            return true;
        }

        public async Task<bool> CreateMatchesByDate(CreateDeleteMatchesModel createDeleteMatchesRequest, TimeSpan offset, IConfiguration configuration)
        {
            var createDate = new DateTime(createDeleteMatchesRequest.EventDate.Year, createDeleteMatchesRequest.EventDate.Month, createDeleteMatchesRequest.EventDate.Day, 0, 0, 0, DateTimeKind.Utc);

            var matchRepository = new MatchRepository(configuration);
            await SimulatorService.SendCleanExistData(configuration);
            await matchRepository.CreateMatches(createDate, createDeleteMatchesRequest.Count, offset, createDeleteMatchesRequest.LeagueLimit);

            return true;
        }


        public async Task<string> CreateMatcheWithForm(IFormCollection collection, TimeSpan offset, IConfiguration configuration)
        {
            var leagueRepo = new LeagueRepository();
            string leagueId = collection["Match.Tournament.Id"];
            var league = await leagueRepo.GetLeagueById(leagueId);

            var teamRepo = new TeamRepository();
            var teams = await teamRepo.GetTeams();

            var newMatch = new MatchEntity
            {
                Id = UniqueGenerator.GetIdWithPattern("sr:match:"),
                Status = collection["Match.Status"],
                MatchStatus = collection["Match.Status"],
                Venue = new Score247Management.Core.Matches.Models.Venue
                {
                    Id = "sr:venue:" + collection["Match.Venue.Name"],
                    Name = collection["Match.Venue.Name"],
                    CountryName = collection["Match.Venue.CountryName"]
                },
                Tournament = league
            };
            var competitors = new List<Score247Management.Core.Teams.Models.Team>();
            var homeTeam = teams.FirstOrDefault(x => x.Id == collection["HomeTeam.Id"]);
            homeTeam.Statistic = new Score247Management.Core.Teams.Models.TeamStatistics();
            homeTeam.qualifier = "home";
            var awayTeam = teams.FirstOrDefault(x => x.Id == collection["AwayTeam.Id"]);
            awayTeam.Statistic = new Score247Management.Core.Teams.Models.TeamStatistics();
            awayTeam.qualifier = "away";
            competitors.Add(homeTeam);
            competitors.Add(awayTeam);
            newMatch.Competitors = competitors;

            var dateTime = Convert.ToDateTime(collection["Match.Scheduled"]);
            var timeSpan = TimeSpan.Parse(collection["Match.ScheduledTime"]);
            newMatch.Scheduled = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds, DateTimeKind.Utc).Subtract(offset);
            newMatch.TournamentRound = new Score247Management.Core.Leagues.Models.LeagueRound
            {
                Type = LeagueRoundType.FromDisplayName<LeagueRoundType>(collection["Match.TournamentRound.Type.Value"]),
                Name = collection["Match.TournamentRound.Name"],
                Number = Int32.Parse(collection["Match.TournamentRound.Number"]),
                Phase = collection["Match.TournamentRound.Phase"],
                Group = collection["Match.TournamentRound.Group"]
            };
            var beginingUTCToday = DateTimeHelper.GetBeginingOfDate(DateTime.UtcNow);
            if (newMatch.Scheduled < beginingUTCToday)
            {
                using (HttpResponseMessage response = await SendCreatePostMatch(configuration, newMatch))
                {
                    if (!response.IsSuccessStatusCode)
                    {
                        return "";
                    }
                }
            }
            else
            {
                using (HttpResponseMessage response = await SendCreatePreMatch(configuration, newMatch))
                {
                    if (!response.IsSuccessStatusCode)
                    {
                        return "";
                    }
                }
            }

            await SendTriggerCreateMatches(configuration, newMatch);

            return newMatch.Id;
        }

        private static async Task SendTriggerCreateMatches(IConfiguration configuration, MatchEntity newMatch)
        {
            await SimulatorService.StartInsertMatchesJobs(configuration);

            var matchRepository = new MatchRepository();
            var matchCreated = await matchRepository.GetMatchById(newMatch.Id);
            while (matchCreated == null)
            {
                await Task.Delay(1000);
                matchCreated = await matchRepository.GetMatchById(newMatch.Id);
            }
        }

        private static async Task<HttpResponseMessage> SendCreatePostMatch(IConfiguration configuration, MatchEntity newMatch)
        {
            var dailyResults = new List<Repositories.Matches.Models.Result>();

            var matchWithResult = new Repositories.Matches.Models.Result()
            {
                sport_event = newMatch,
                sport_event_status = GenerateDefaultEventStatusFromMatchEntity(newMatch)
            };

            dailyResults.Add(matchWithResult);
            var dailyResultRequest = new Repositories.Matches.Models.DailyResultDto
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
                var MatchEntityes = new List<MatchEntity> { newMatch };
                var content = new Repositories.Matches.Models.DailyScheduleDto
                {
                    generated_at = DateTime.UtcNow,
                    schema = "Matches",
                    sport_events = MatchEntityes
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

        public async Task<string> AutoCreateMatch(DateTimeOffset? dateTimeOffset, IConfiguration configuration)
        {
            var matchRepository = new MatchRepository(configuration);

            return await matchRepository.AutoCreateMatche(dateTimeOffset);
        }
    }
}