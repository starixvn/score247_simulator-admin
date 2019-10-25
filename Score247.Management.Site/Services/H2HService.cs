namespace Score247.Management.Site.Services
{
    using System.Linq;
    using System.Threading.Tasks;
    using Score247.Management.Site.Models;
    using Score247.Management.Site.Models.DataMappers;
    using Score247.Management.Site.Repositories.Matches;

    public interface IH2HService
    {
        Task<H2HDto> GenerateH2HData(string matchId);
    }

    public class H2HService : IH2HService
    {
        private readonly IMatchFileService matchFileService;

        public H2HService(IMatchFileService matchFileService)
        {
            this.matchFileService = matchFileService;
        }

        public async Task<H2HDto> GenerateH2HData(string matchId)
        {
            var matchRepository = new MatchRepository();
            var match = await matchRepository.GetMatchById(matchId);
            var h2hDto = await matchFileService.ReadH2HFromFile("default");
            var oldHomeId = h2hDto.teams.ToList()[0].id;
            var oldAwayId = h2hDto.teams.ToList()[1].id;
            var competitors = match.Teams.Select(SportEventMapper.MapCompetitor).ToList();
            var newHomeTeam = competitors.FirstOrDefault(x => x.qualifier == "home");
            var newAwayTeam = competitors.FirstOrDefault(x => x.qualifier == "away");

            foreach (var result in h2hDto.last_meetings.results)
            {
                foreach (var competitor in result.sport_event.competitors)
                {
                    if (competitor.id == oldHomeId)
                    {
                        competitor.id = newHomeTeam.id;
                        competitor.name = newHomeTeam.name;
                        competitor.country = newHomeTeam.country;
                        competitor.country_code = newHomeTeam.country_code;
                        competitor.abbreviation = newHomeTeam.abbreviation;
                        competitor.qualifier = newHomeTeam.qualifier;
                    }
                    else
                    {
                        competitor.id = newAwayTeam.id;
                        competitor.name = newAwayTeam.name;
                        competitor.country = newAwayTeam.country;
                        competitor.country_code = newAwayTeam.country_code;
                        competitor.abbreviation = newAwayTeam.abbreviation;
                        competitor.qualifier = newAwayTeam.qualifier;
                    }
                }

                if (oldHomeId == result.sport_event_status.winner_id)
                {
                    result.sport_event_status.winner_id = newHomeTeam.id;
                }
                else if (oldAwayId == result.sport_event_status.winner_id)
                {
                    result.sport_event_status.winner_id = newAwayTeam.id;
                }

                if (oldHomeId == result.sport_event_status.aggregate_winner_id)
                {
                    result.sport_event_status.aggregate_winner_id = newHomeTeam.id;
                }
                else if (oldAwayId == result.sport_event_status.aggregate_winner_id)
                {
                    result.sport_event_status.aggregate_winner_id = newAwayTeam.id;
                }
            }

            foreach (var result in h2hDto.next_meetings)
            {
                foreach (var competitor in result.sport_event.competitors)
                {
                    if (competitor.id == oldHomeId)
                    {
                        competitor.id = newHomeTeam.id;
                        competitor.name = newHomeTeam.name;
                        competitor.country = newHomeTeam.country;
                        competitor.country_code = newHomeTeam.country_code;
                        competitor.abbreviation = newHomeTeam.abbreviation;
                        competitor.qualifier = newHomeTeam.qualifier;
                    }
                    else
                    {
                        competitor.id = newAwayTeam.id;
                        competitor.name = newAwayTeam.name;
                        competitor.country = newAwayTeam.country;
                        competitor.country_code = newAwayTeam.country_code;
                        competitor.abbreviation = newAwayTeam.abbreviation;
                        competitor.qualifier = newAwayTeam.qualifier;
                    }
                }
            }

            foreach (var competitor in competitors)
            {
                competitor.qualifier = null;
            }
            h2hDto.teams = competitors;

            return h2hDto;
        }
    }
}