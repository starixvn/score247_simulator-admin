using System.Linq;
using Score247.Management.Site.Models.Events;

namespace Score247.Management.Site.Models.DataMappers
{
    public static class TimelineMapper
    {
        public static MatchTimelineDto MapTimelinetoSportEvent(Sport_Event sportEvent, MatchTimelineDto timeline)
        {
            var homeTeam = sportEvent.competitors.FirstOrDefault(x => x.qualifier == "home");
            var awayTeam = sportEvent.competitors.FirstOrDefault(x => x.qualifier != "home");

            var tlHomeTeam = timeline.sport_event.competitors.FirstOrDefault(x => x.qualifier == "home");
            var tlAwayTeam = timeline.sport_event.competitors.FirstOrDefault(x => x.qualifier != "home");

            if (timeline.sport_event_status.aggregate_winner_id == tlHomeTeam.id)
            {
                timeline.sport_event_status.aggregate_winner_id = homeTeam.id;
            }
            if (timeline.sport_event_status.aggregate_winner_id == tlAwayTeam.id)
            {
                timeline.sport_event_status.aggregate_winner_id = awayTeam.id;
            }
            if (timeline.sport_event_status.winner_id == tlHomeTeam.id)
            {
                timeline.sport_event_status.winner_id = homeTeam.id;
            }
            if (timeline.sport_event_status.winner_id == tlAwayTeam.id)
            {
                timeline.sport_event_status.winner_id = awayTeam.id;
            }

            timeline.sport_event = sportEvent;

            return timeline;
        }
    }
}