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
                Venue = new Score247Management.Core.Matches.Models.Venue
                {
                    Id = "sr:venue:" + collection["Match.Venue.Name"],
                    Name = collection["Match.Venue.Name"],
                    CountryName = collection["Match.Venue.CountryName"]
                },
                Tournament = league
            };
            var competitors = new List<Team>();
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

                using (HttpResponseMessage response = await client.PostAsync(string.Format(requestPath + "?date={0}", newMatch.Scheduled.ToString("yyyy-MM-dd")), new StringContent(jsonValue, Encoding.UTF8, "application/json")))
                {
                    if (!response.IsSuccessStatusCode)
                    {
                        return "";
                    }
                }

                await SimulatorService.StartInsertMatchesJobs(configuration);

                var matchRepository = new MatchRepository();
                var matchCreated = await matchRepository.GetMatchById(newMatch.Id);
                while (matchCreated == null)
                {
                    await Task.Delay(1000);
                    matchCreated = await matchRepository.GetMatchById(newMatch.Id);
                }
            }

            return newMatch.Id;
        }
    }
}