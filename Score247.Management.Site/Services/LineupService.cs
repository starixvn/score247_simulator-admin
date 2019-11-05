using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Score247.Management.Site.Helpers;
using Score247.Management.Site.Models;
using Score247.Management.Site.Models.DataMappers;
using Score247.Management.Site.Models.Events;
using Score247Management.Core.Matches.Models;
using Score247Management.Core.Teams.Models;

namespace Score247.Management.Site.Services
{
    public interface ILineupService
    {
        LineUpsDto BuildLineups(Match match, string homeFormation, string awayFormation);

        LineUpsDto BuildLineups(Sport_Event match, string homeFormation, string awayFormation);
    }

    public class LineupService : ILineupService
    {
        private static readonly string DirectoryRoot = AppDomain.CurrentDomain.BaseDirectory + "/App_Data/lineups";

        public LineUpsDto BuildLineups(Match match, string homeFormation, string awayFormation)
        {
            var homeTeam = match.Teams.FirstOrDefault(team => team.IsHome);
            var awayTeam = match.Teams.FirstOrDefault(team => !team.IsHome);

            var homeLineup = BuildTeamLineup(homeTeam, homeFormation);
            var awayLineup = BuildTeamLineup(awayTeam, awayFormation);

            var lineupsDto = new LineUpsDto();
            lineupsDto.generated_at = DateTime.Now;
            lineupsDto.schema = "";
            lineupsDto.sport_event = SportEventMapper.MapSportEvent(match);
            lineupsDto.lineups = new List<LineUp> { homeLineup, awayLineup };

            return lineupsDto;
        }

        public LineUpsDto BuildLineups(Sport_Event match, string homeFormation, string awayFormation)
        {
            var homeTeam = match.competitors.FirstOrDefault(team => team.qualifier == "home");
            var awayTeam = match.competitors.FirstOrDefault(team => team.qualifier == "away");

            var homeLineup = BuildTeamLineup(homeTeam, homeFormation);
            var awayLineup = BuildTeamLineup(awayTeam, awayFormation);

            var lineupsDto = new LineUpsDto();
            lineupsDto.generated_at = DateTime.Now;
            lineupsDto.schema = "";
            lineupsDto.sport_event = match;
            lineupsDto.lineups = new List<LineUp> { homeLineup, awayLineup };

            return lineupsDto;
        }

        private static LineUp BuildTeamLineup(Score247Management.Core.Teams.Models.Team team, string formation)
        {
            var lineup = GetFormationData(formation);
            lineup.team = team.IsHome ? "home" : "away";

            return lineup;
        }

        private static LineUp BuildTeamLineup(Competitor team, string formation)
        {
            var lineup = GetFormationData(formation);
            lineup.team = team.qualifier;

            return lineup;
        }

        private static LineUp GetFormationData(string formation)
        {
            try
            {
                var resultFolder = $"{DirectoryRoot}";
                Directory.CreateDirectory(resultFolder);

                var path = $"{resultFolder}/{formation}.json";
                var jsonData = File.ReadAllText(path);
                var content = JsonConvert.DeserializeObject<LineUp>(jsonData);
                foreach (var player in content.starting_lineup)
                {
                    player.id = UniqueGenerator.GetIdWithPattern("sr:player:");
                    player.name = NameGenerator.GeneratePlayerName();
                }
                foreach (var player in content.substitutes)
                {
                    player.id = UniqueGenerator.GetIdWithPattern("sr:player:");
                    player.name = NameGenerator.GeneratePlayerName();
                }
                content.manager.name = NameGenerator.GeneratePlayerName();
                return content;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}