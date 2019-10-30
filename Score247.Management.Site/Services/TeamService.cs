namespace Score247.Management.Site.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Score247.Management.Site.Helpers;
    using Score247.Management.Site.Models;
    using Score247.Management.Site.Models.DataMappers;
    using Score247.Management.Site.Repositories.Matches;

    public interface ITeamService
    {
        Task ApplyTeamResult(TeamResultDto teamResultDto);

        Task<TeamResultDto> GenerateTeamResult(string matchId, string teamQualifier, string teamResultCase);
    }

    public class TeamService : ITeamService
    {
        private readonly IMatchFileService matchFileService;
        private readonly ISimulatorService simulatorService;
        private readonly ITriggerJobService triggerJobService;

        public TeamService(IMatchFileService matchFileService,
            ISimulatorService simulatorService,
            ITriggerJobService triggerJobService)
        {
            this.matchFileService = matchFileService;
            this.simulatorService = simulatorService;
            this.triggerJobService = triggerJobService;
        }

        public async Task ApplyTeamResult(TeamResultDto teamResultDto)
        {
            await simulatorService.SendTeamResult(teamResultDto);
            triggerJobService.TriggerCreateMatchJobs();
        }

        public async Task<TeamResultDto> GenerateTeamResult(string matchId, string teamQualifier, string teamResultCase)
        {
            var matchRepository = new MatchRepository();
            var match = await matchRepository.GetMatchById(matchId);
            var teamResultDto = await matchFileService.ReadTeamResult(teamResultCase);
            var newTeam = match.Teams.Select(SportEventMapper.MapCompetitor).FirstOrDefault(team => team.qualifier == teamQualifier);

            var oldTeamId = teamResultDto.team.id;

            foreach (var result in teamResultDto.results)
            {
                result.sport_event.id = UniqueGenerator.GetIdWithPattern("sr:match:");
                foreach (var competitor in result.sport_event.competitors)
                {
                    if (competitor.id == oldTeamId)
                    {
                        competitor.id = newTeam.id;
                        competitor.name = newTeam.name;
                        competitor.country = newTeam.country;
                        competitor.country_code = newTeam.country_code;
                        competitor.abbreviation = newTeam.abbreviation;
                    }
                }

                if (oldTeamId == result.sport_event_status.winner_id)
                {
                    result.sport_event_status.winner_id = newTeam.id;
                }

                if (oldTeamId == result.sport_event_status.aggregate_winner_id)
                {
                    result.sport_event_status.aggregate_winner_id = newTeam.id;
                }
            }

            newTeam.qualifier = null;
            teamResultDto.team = newTeam;

            return teamResultDto;
        }
    }
}