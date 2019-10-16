using Newtonsoft.Json;
using Score247.Management.Site.Helpers;
using Score247.Management.Site.Models.Events;
using Score247.Management.Site.Repositories.Matches.Models;
using Score247Management.Core.Leagues.Models;
using Score247Management.Core.Matches.Models.Soccer.Core.Matches.Models;
using Score247Management.Core.Scenarios.Models;

namespace Score247.Management.Site.Models.DataMappers
{
    using System.Linq;
    using Score247Management.Core.Matches.Models;
    using Score247Management.Core.Teams.Models;

    public static class SportEventMapper
    {
        public static MatchPushEventDto MapEvent(ScenarioEvent scenarioEvent, Match match, Scenario scenario)
        {
            var homeId = match.Teams.FirstOrDefault(x => x.IsHome)?.Id;
            var awayId = match.Teams.FirstOrDefault(x => !x.IsHome)?.Id;
            var differTime = match.EventDate - scenario.EventDate;
            var preEvent = JsonConvert.DeserializeObject<MatchPushEventDto>(scenarioEvent.EventRequest);
            preEvent.payload.timeline.id = UniqueGenerator.GetIntId();
            preEvent.metadata.sport_event_id = match.Id;
            preEvent.metadata.tournament_id = match.League.Id;
            preEvent.payload.timeline.time = preEvent.payload.timeline.time + differTime;
            if (preEvent.payload.sport_event_status.aggregate_winner_id == scenario.HomeId) preEvent.payload.sport_event_status.aggregate_winner_id = homeId;
            if (preEvent.payload.sport_event_status.winner_id == scenario.HomeId) preEvent.payload.sport_event_status.winner_id = homeId;
            if (preEvent.payload.sport_event_status.aggregate_winner_id == scenario.AwayId) preEvent.payload.sport_event_status.aggregate_winner_id = awayId;
            if (preEvent.payload.sport_event_status.winner_id == scenario.AwayId) preEvent.payload.sport_event_status.winner_id = awayId;

            return preEvent;
        }

        public static Sport_Event MapSportEvent(MatchEntity match)
        {
            var sportEvent = new Sport_Event
            {
                id = match.Id,
                competitors = match.Competitors.Select(MapCompetitor),
                scheduled = match.Scheduled,
                tournament = MapTournament(match.Tournament),
                venue = MapVenueDto(match.Venue),
            };

            return sportEvent;
        }

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

        public static Competitor MapCompetitor(Team team)
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

        public static Tournament MapTournament(League league)
        {
            var tournament = new Tournament
            {
                id = league.Id,
                category = new Category() { country_code = league.CountryCode, name = league.Country }
            };

            return tournament;
        }

        public static VenueDto MapVenueDto(Venue venue)
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

        public static CoverageInfoDto MapCoverageInfoDto(Coverage coverage)
        {
            var coverageInfoDto = new CoverageInfoDto
            {
                live_coverage = coverage?.Live ?? false,
                level = "bronze",
                coverage = new CoverageDto()
                {
                    commentary = coverage?.Commentary ?? false,
                    basic_score = coverage?.BasicScore ?? false,
                    key_events = coverage?.KeyEvents ?? false,
                    detailed_events = coverage?.DetailedEvents ?? false,
                    lineups = coverage?.Lineups ?? false
                }
            };

            return coverageInfoDto;
        }
    }
}