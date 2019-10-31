namespace Score247.Management.Site.Services
{
    using System.Linq;
    using System.Threading.Tasks;
    using Score247.Management.Site.Helpers;
    using Score247.Management.Site.Models;
    using Score247.Management.Site.Models.DataMappers;
    using Score247.Management.Site.Models.Events;
    using Score247.Management.Site.Repositories.Matches;
    using Score247Management.Core.Matches.Models;

    public interface IH2HService
    {
        Task<H2HDto> GenerateH2HData(string matchId, string h2hCase = "default");

        Task<H2HDto> GenerateH2HData(Match match, string h2hCase = "default");

        Task<H2HDto> GenerateH2HData(Sport_Event match, string h2hCase = "default");
    }

    public class H2HService : IH2HService
    {
        private readonly IMatchFileService matchFileService;

        public H2HService(IMatchFileService matchFileService)
        {
            this.matchFileService = matchFileService;
        }

        public async Task<H2HDto> GenerateH2HData(string matchId, string h2hCase = "default")
        {
            var matchRepository = new MatchRepository();
            var match = await matchRepository.GetMatchById(matchId);

            return await GenerateH2HData(match, h2hCase);
        }

        public async Task<H2HDto> GenerateH2HData(Match match, string h2hCase = "default")
        {
            var sportEvent = SportEventMapper.MapSportEvent(match);

            return await GenerateH2HData(sportEvent, h2hCase);
        }

        public async Task<H2HDto> GenerateH2HData(Sport_Event match, string h2hCase = "default")
        {
            var h2hDto = await matchFileService.ReadH2HFromFile(h2hCase);
            var oldHomeId = h2hDto.teams.ToList()[0].id;
            var oldAwayId = h2hDto.teams.ToList()[1].id;
            var competitors = match.competitors.ToList();
            var newHomeTeam = competitors.FirstOrDefault(x => x.qualifier == "home");
            var newAwayTeam = competitors.FirstOrDefault(x => x.qualifier == "away");

            foreach (var result in h2hDto.last_meetings.results)
            {
                result.sport_event.id = UniqueGenerator.GetIdWithPattern("sr:match:");
                foreach (var competitor in result.sport_event.competitors)
                {
                    if (competitor.id == oldHomeId)
                    {
                        competitor.id = newHomeTeam.id;
                        competitor.name = newHomeTeam.name;
                        competitor.country = newHomeTeam.country;
                        competitor.country_code = newHomeTeam.country_code;
                        competitor.abbreviation = newHomeTeam.abbreviation;
                    }
                    else
                    {
                        competitor.id = newAwayTeam.id;
                        competitor.name = newAwayTeam.name;
                        competitor.country = newAwayTeam.country;
                        competitor.country_code = newAwayTeam.country_code;
                        competitor.abbreviation = newAwayTeam.abbreviation;
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
                result.sport_event.id = UniqueGenerator.GetIdWithPattern("sr:match:");
                foreach (var competitor in result.sport_event.competitors)
                {
                    if (competitor.id == oldHomeId)
                    {
                        competitor.id = newHomeTeam.id;
                        competitor.name = newHomeTeam.name;
                        competitor.country = newHomeTeam.country;
                        competitor.country_code = newHomeTeam.country_code;
                        competitor.abbreviation = newHomeTeam.abbreviation;
                    }
                    else
                    {
                        competitor.id = newAwayTeam.id;
                        competitor.name = newAwayTeam.name;
                        competitor.country = newAwayTeam.country;
                        competitor.country_code = newAwayTeam.country_code;
                        competitor.abbreviation = newAwayTeam.abbreviation;
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