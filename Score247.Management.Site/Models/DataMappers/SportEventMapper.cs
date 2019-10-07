using Score247.Management.Site.Models.Events;
using Score247Management.Core.Leagues.Models;

namespace Score247.Management.Site.Models.DataMappers
{
    using System.Linq;
    using Score247Management.Core.Matches.Models;
    using Score247Management.Core.Teams.Models;

    public static class SportEventMapper
    {
        public static Sport_Event MapSportEvent(Match match)
        {
            var sportEvent = new Sport_Event
            {
                id = match.Id,
                competitors = match.Teams.Select(MapCompetitor),
                scheduled = match.EventDate,
                tournament = MapTournament(match.League),
                venue = MapVenueDto(match.Venue)
            };

            return sportEvent;
        }

        private static Competitor MapCompetitor(Team team)
        {
            var competitor = new Competitor
            {
                id = team.Id,
                abbreviation = team.Abbreviation,
                country = team.Country,
                country_code = team.CountryCode,
                name = team.Name,
                qualifier = team.qualifier
            };

            return competitor;
        }

        private static Tournament MapTournament(League league)
        {
            var tournament = new Tournament
            {
                id = league.Id,
                category = new Category() { country_code = league.CountryCode, name = league.Country }
            };

            return tournament;
        }

        private static VenueDto MapVenueDto(Venue venue)
        {
            var venueDto = new VenueDto
            {
                id = venue.Id,
                capacity = venue.Capacity,
                country_code = venue.CountryCode,
                name = venue.Name,
                city_name = venue.CityName,
                country_name = venue.CountryName
            };

            return venueDto;
        }
    }
}