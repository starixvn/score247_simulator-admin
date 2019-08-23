using System;
using System.Configuration;
using System.Net.Http;
using System.Threading.Tasks;
using Score247.Management.Site.Models.Events;
using Score247.Management.Site.Repositories.Match;
using Score247Management.Core.Matches.Models;

namespace Score247.Management.Site.Services.Matchs
{
    public static class AutoEventService
    {
        public static async Task PushNormalMatchEvents(string matchId, string baseUrl, string eventPath)
        {
            int currentMatchTime = 0;
            int breakHalfTime = 15;
            int extraTime = 0;
            var rand = new Random();

            var matchRepository = new MatchRepository();
            var match = await matchRepository.GetMatchById(matchId);

            // Match Start
            var eventMatch = MatchTimeEventCreator.CreateMatchStart(match, match.EventDate.DateTime);
            await SendEvent(eventMatch, baseUrl, eventPath);
            match = await matchRepository.GetMatchById(matchId);

            // First Halft Start
            eventMatch = MatchTimeEventCreator.CreateFirstPeriodStart(match, match.EventDate.DateTime);
            await SendEvent(eventMatch, baseUrl, eventPath);
            match = await matchRepository.GetMatchById(matchId);

            // Card Event
            currentMatchTime = 15;
            eventMatch = MatchCardEventCreator.CreateYellowCardEvent(match, match.EventDate.DateTime.AddMinutes(currentMatchTime), currentMatchTime, "home", "Player_" + rand.Next(999).ToString(), 0);
            await SendEvent(eventMatch, baseUrl, eventPath);
            match = await matchRepository.GetMatchById(matchId);

            // Score event
            currentMatchTime = 20;
            eventMatch = MatchScoreEventCreator.CreateNormalScore(match, match.EventDate.DateTime.AddMinutes(currentMatchTime), currentMatchTime, "home", "Player_" + rand.Next(999).ToString(), false);
            await SendEvent(eventMatch, baseUrl, eventPath);
            match = await matchRepository.GetMatchById(matchId);

            // Own Score
            currentMatchTime = 30;
            eventMatch = MatchScoreEventCreator.CreateNormalScore(match, match.EventDate.DateTime.AddMinutes(currentMatchTime), currentMatchTime, "home", "Player_" + rand.Next(999).ToString(), true);
            await SendEvent(eventMatch, baseUrl, eventPath);
            match = await matchRepository.GetMatchById(matchId);


            // Extra Injury Time Start
            currentMatchTime = 45;
            extraTime = 2;
            eventMatch = MatchInjuryExtraEventCreator.CreateFirstHalfInjuryExtraEvent(match, match.EventDate.DateTime.AddMinutes(currentMatchTime), extraTime);
            await SendEvent(eventMatch, baseUrl, eventPath);
            match = await matchRepository.GetMatchById(matchId);

            // Card Event
            eventMatch = MatchCardEventCreator.CreateRedCardEvent(match, match.EventDate.DateTime.AddMinutes(currentMatchTime + 1), currentMatchTime, "away", "Player_" + rand.Next(999).ToString(), 1);
            await SendEvent(eventMatch, baseUrl, eventPath);
            match = await matchRepository.GetMatchById(matchId);

            // Break Start
            eventMatch = MatchTimeEventCreator.CreateHalfTimeBreakStart(match, match.EventDate.DateTime.AddMinutes(currentMatchTime + extraTime));
            await SendEvent(eventMatch, baseUrl, eventPath);
            match = await matchRepository.GetMatchById(matchId);

            // Second Half Start
            currentMatchTime = 46;
            eventMatch = MatchTimeEventCreator.CreateSecondPeriodStart(match, match.EventDate.DateTime.AddMinutes(currentMatchTime + extraTime + breakHalfTime));
            await SendEvent(eventMatch, baseUrl, eventPath);
            match = await matchRepository.GetMatchById(matchId);

            // Card Event
            currentMatchTime = 49;
            eventMatch = MatchCardEventCreator.CreateYellowRedCardEvent(match, match.EventDate.DateTime.AddMinutes(currentMatchTime + extraTime + breakHalfTime), currentMatchTime, "away", "Player_" + rand.Next(999).ToString(), 0);
            await SendEvent(eventMatch, baseUrl, eventPath);
            match = await matchRepository.GetMatchById(matchId);

            // Score event
            currentMatchTime = 52;
            eventMatch = MatchScoreEventCreator.CreatePenaltyScore(match, match.EventDate.DateTime.AddMinutes(currentMatchTime + extraTime + breakHalfTime), currentMatchTime, "away", "Player_" + rand.Next(999).ToString());
            await SendEvent(eventMatch, baseUrl, eventPath);
            match = await matchRepository.GetMatchById(matchId);

            // Extra Injury Time Start
            currentMatchTime = 90;
            extraTime = 5;
            eventMatch = MatchInjuryExtraEventCreator.CreateSecondHalfInjuryExtraEvent(match, match.EventDate.DateTime.AddMinutes(currentMatchTime + breakHalfTime + 3), 3);
            await SendEvent(eventMatch, baseUrl, eventPath);
            match = await matchRepository.GetMatchById(matchId);

            // Card Event
            eventMatch = MatchCardEventCreator.CreateYellowCardEvent(match, match.EventDate.DateTime.AddMinutes(currentMatchTime + breakHalfTime + 3), currentMatchTime, "away", "Player_" + rand.Next(999).ToString(), 2);
            await SendEvent(eventMatch, baseUrl, eventPath);
            match = await matchRepository.GetMatchById(matchId);

            // Match End Full Time
            eventMatch = MatchTimeEventCreator.CreateMatchEndNormal(match, match.EventDate.DateTime.AddMinutes(currentMatchTime + breakHalfTime + extraTime));
            await SendEvent(eventMatch, baseUrl, eventPath);

        }
        public static async Task PushOvertimeMatchEvents(string matchId, string baseUrl, string eventPath)
        {
            int currentMatchTime = 0;
            int breakHalfTime = 15;
            int extraTime = 0;
            var rand = new Random();

            var matchRepository = new MatchRepository();
            var match = await matchRepository.GetMatchById(matchId);

            // Match Start
            var eventMatch = MatchTimeEventCreator.CreateMatchStart(match, match.EventDate.DateTime);
            await SendEvent(eventMatch, baseUrl, eventPath);
            match = await matchRepository.GetMatchById(matchId);

            // First Halft Start
            eventMatch = MatchTimeEventCreator.CreateFirstPeriodStart(match, match.EventDate.DateTime);
            await SendEvent(eventMatch, baseUrl, eventPath);
            match = await matchRepository.GetMatchById(matchId);

            // Card Event
            currentMatchTime = 15;
            eventMatch = MatchCardEventCreator.CreateYellowCardEvent(match, match.EventDate.DateTime.AddMinutes(currentMatchTime), currentMatchTime, "home", "Player_" + rand.Next(999).ToString(), 0);
            await SendEvent(eventMatch, baseUrl, eventPath);
            match = await matchRepository.GetMatchById(matchId);

            // Score event
            currentMatchTime = 20;
            eventMatch = MatchScoreEventCreator.CreateNormalScore(match, match.EventDate.DateTime.AddMinutes(currentMatchTime), currentMatchTime, "home", "Player_" + rand.Next(999).ToString(), false);
            await SendEvent(eventMatch, baseUrl, eventPath);
            match = await matchRepository.GetMatchById(matchId);

            // Own Score
            currentMatchTime = 30;
            eventMatch = MatchScoreEventCreator.CreateNormalScore(match, match.EventDate.DateTime.AddMinutes(currentMatchTime), currentMatchTime, "home", "Player_" + rand.Next(999).ToString(), true);
            await SendEvent(eventMatch, baseUrl, eventPath);
            match = await matchRepository.GetMatchById(matchId);


            // Extra Injury Time Start
            currentMatchTime = 45;
            extraTime = 2;
            eventMatch = MatchInjuryExtraEventCreator.CreateFirstHalfInjuryExtraEvent(match, match.EventDate.DateTime.AddMinutes(currentMatchTime), extraTime);
            await SendEvent(eventMatch, baseUrl, eventPath);
            match = await matchRepository.GetMatchById(matchId);

            // Card Event
            eventMatch = MatchCardEventCreator.CreateRedCardEvent(match, match.EventDate.DateTime.AddMinutes(currentMatchTime + 1), currentMatchTime, "away", "Player_" + rand.Next(999).ToString(), 1);
            await SendEvent(eventMatch, baseUrl, eventPath);
            match = await matchRepository.GetMatchById(matchId);

            // Break Start
            eventMatch = MatchTimeEventCreator.CreateHalfTimeBreakStart(match, match.EventDate.DateTime.AddMinutes(currentMatchTime + extraTime));
            await SendEvent(eventMatch, baseUrl, eventPath);
            match = await matchRepository.GetMatchById(matchId);

            // Second Half Start
            currentMatchTime = 46;
            eventMatch = MatchTimeEventCreator.CreateSecondPeriodStart(match, match.EventDate.DateTime.AddMinutes(currentMatchTime + extraTime + breakHalfTime));
            await SendEvent(eventMatch, baseUrl, eventPath);
            match = await matchRepository.GetMatchById(matchId);

            // Card Event
            currentMatchTime = 49;
            eventMatch = MatchCardEventCreator.CreateYellowRedCardEvent(match, match.EventDate.DateTime.AddMinutes(currentMatchTime + extraTime + breakHalfTime), currentMatchTime, "away", "Player_" + rand.Next(999).ToString(), 0);
            await SendEvent(eventMatch, baseUrl, eventPath);
            match = await matchRepository.GetMatchById(matchId);

            // Score event
            currentMatchTime = 52;
            eventMatch = MatchScoreEventCreator.CreatePenaltyScore(match, match.EventDate.DateTime.AddMinutes(currentMatchTime + extraTime + breakHalfTime), currentMatchTime, "away", "Player_" + rand.Next(999).ToString());
            await SendEvent(eventMatch, baseUrl, eventPath);
            match = await matchRepository.GetMatchById(matchId);

            // Extra Injury Time Start
            currentMatchTime = 90;
            extraTime = 5;
            eventMatch = MatchInjuryExtraEventCreator.CreateSecondHalfInjuryExtraEvent(match, match.EventDate.DateTime.AddMinutes(currentMatchTime + breakHalfTime + 3), 3);
            await SendEvent(eventMatch, baseUrl, eventPath);
            match = await matchRepository.GetMatchById(matchId);

            // Card Event
            eventMatch = MatchCardEventCreator.CreateYellowCardEvent(match, match.EventDate.DateTime.AddMinutes(currentMatchTime + breakHalfTime + 3), currentMatchTime, "away", "Player_" + rand.Next(999).ToString(), 2);
            await SendEvent(eventMatch, baseUrl, eventPath);
            match = await matchRepository.GetMatchById(matchId);

            // Break Awaiting Extra
            var awaitingExtra = 10;
            eventMatch = MatchTimeEventCreator.CreateAwaitingExtra(match, match.EventDate.DateTime.AddMinutes(currentMatchTime + breakHalfTime + extraTime));
            await SendEvent(eventMatch, baseUrl, eventPath);
            match = await matchRepository.GetMatchById(matchId);

            //First Extra Period Start
            eventMatch = MatchTimeEventCreator.CreateFirstExtraStart(match, match.EventDate.DateTime.AddMinutes(currentMatchTime + breakHalfTime + extraTime + awaitingExtra));
            await SendEvent(eventMatch, baseUrl, eventPath);
            match = await matchRepository.GetMatchById(matchId);

            // Card Event
            eventMatch = MatchCardEventCreator.CreateYellowCardEvent(match, match.EventDate.DateTime.AddMinutes(currentMatchTime + breakHalfTime + 3), currentMatchTime, "away", "Player_" + rand.Next(999).ToString(), 2);
            await SendEvent(eventMatch, baseUrl, eventPath);
            match = await matchRepository.GetMatchById(matchId);

            // Break Awaiting Extra
            currentMatchTime = 105;
            var breakExtraTime = 5;
            eventMatch = MatchTimeEventCreator.CreateBreakExtraStart(match, match.EventDate.DateTime.AddMinutes(currentMatchTime + breakHalfTime + extraTime));
            await SendEvent(eventMatch, baseUrl, eventPath);
            match = await matchRepository.GetMatchById(matchId);

            // Break Awaiting Extra
            eventMatch = MatchTimeEventCreator.CreateSecondExtraStart(match, match.EventDate.DateTime.AddMinutes(currentMatchTime + breakHalfTime + extraTime + breakExtraTime));
            await SendEvent(eventMatch, baseUrl, eventPath);
            match = await matchRepository.GetMatchById(matchId);

            // Score event
            currentMatchTime = 115;
            eventMatch = MatchScoreEventCreator.CreateNormalScore(match, match.EventDate.DateTime.AddMinutes(currentMatchTime + breakHalfTime + extraTime + breakExtraTime), currentMatchTime, "home", "Player_" + rand.Next(999).ToString(), false);
            await SendEvent(eventMatch, baseUrl, eventPath);
            match = await matchRepository.GetMatchById(matchId);

            // Match event
            currentMatchTime = 120;
            eventMatch = MatchTimeEventCreator.CreateMatchEndExtra(match, match.EventDate.DateTime.AddMinutes(currentMatchTime + breakHalfTime + extraTime + breakExtraTime));
            await SendEvent(eventMatch, baseUrl, eventPath);

        }
        public static async Task PushPenaltyMatchEvents(string matchId, string baseUrl, string eventPath)
        {

            int currentMatchTime = 0;
            int breakHalfTime = 15;
            int extraTime = 0;
            var rand = new Random();

            var matchRepository = new MatchRepository();
            var match = await matchRepository.GetMatchById(matchId);

            // Match Start
            var eventMatch = MatchTimeEventCreator.CreateMatchStart(match, match.EventDate.DateTime);
            await SendEvent(eventMatch, baseUrl, eventPath);
            match = await matchRepository.GetMatchById(matchId);

            // First Halft Start
            eventMatch = MatchTimeEventCreator.CreateFirstPeriodStart(match, match.EventDate.DateTime);
            await SendEvent(eventMatch, baseUrl, eventPath);
            match = await matchRepository.GetMatchById(matchId);

            // Card Event
            currentMatchTime = 15;
            eventMatch = MatchCardEventCreator.CreateYellowCardEvent(match, match.EventDate.DateTime.AddMinutes(currentMatchTime), currentMatchTime, "home", "Player_" + rand.Next(999).ToString(), 0);
            await SendEvent(eventMatch, baseUrl, eventPath);
            match = await matchRepository.GetMatchById(matchId);

            // Score event
            currentMatchTime = 20;
            eventMatch = MatchScoreEventCreator.CreateNormalScore(match, match.EventDate.DateTime.AddMinutes(currentMatchTime), currentMatchTime, "home", "Player_" + rand.Next(999).ToString(), false);
            await SendEvent(eventMatch, baseUrl, eventPath);
            match = await matchRepository.GetMatchById(matchId);

            // Own Score
            currentMatchTime = 30;
            eventMatch = MatchScoreEventCreator.CreateNormalScore(match, match.EventDate.DateTime.AddMinutes(currentMatchTime), currentMatchTime, "home", "Player_" + rand.Next(999).ToString(), true);
            await SendEvent(eventMatch, baseUrl, eventPath);
            match = await matchRepository.GetMatchById(matchId);


            // Extra Injury Time Start
            currentMatchTime = 45;
            extraTime = 2;
            eventMatch = MatchInjuryExtraEventCreator.CreateFirstHalfInjuryExtraEvent(match, match.EventDate.DateTime.AddMinutes(currentMatchTime), extraTime);
            await SendEvent(eventMatch, baseUrl, eventPath);
            match = await matchRepository.GetMatchById(matchId);

            // Card Event
            eventMatch = MatchCardEventCreator.CreateRedCardEvent(match, match.EventDate.DateTime.AddMinutes(currentMatchTime + 1), currentMatchTime, "away", "Player_" + rand.Next(999).ToString(), 1);
            await SendEvent(eventMatch, baseUrl, eventPath);
            match = await matchRepository.GetMatchById(matchId);

            // Break Start
            eventMatch = MatchTimeEventCreator.CreateHalfTimeBreakStart(match, match.EventDate.DateTime.AddMinutes(currentMatchTime + extraTime));
            await SendEvent(eventMatch, baseUrl, eventPath);
            match = await matchRepository.GetMatchById(matchId);

            // Second Half Start
            currentMatchTime = 46;
            eventMatch = MatchTimeEventCreator.CreateSecondPeriodStart(match, match.EventDate.DateTime.AddMinutes(currentMatchTime + extraTime + breakHalfTime));
            await SendEvent(eventMatch, baseUrl, eventPath);
            match = await matchRepository.GetMatchById(matchId);

            // Card Event
            currentMatchTime = 49;
            eventMatch = MatchCardEventCreator.CreateYellowRedCardEvent(match, match.EventDate.DateTime.AddMinutes(currentMatchTime + extraTime + breakHalfTime), currentMatchTime, "away", "Player_" + rand.Next(999).ToString(), 0);
            await SendEvent(eventMatch, baseUrl, eventPath);
            match = await matchRepository.GetMatchById(matchId);

            // Score event
            currentMatchTime = 52;
            eventMatch = MatchScoreEventCreator.CreatePenaltyScore(match, match.EventDate.DateTime.AddMinutes(currentMatchTime + extraTime + breakHalfTime), currentMatchTime, "away", "Player_" + rand.Next(999).ToString());
            await SendEvent(eventMatch, baseUrl, eventPath);
            match = await matchRepository.GetMatchById(matchId);

            // Extra Injury Time Start
            currentMatchTime = 90;
            extraTime = 5;
            eventMatch = MatchInjuryExtraEventCreator.CreateSecondHalfInjuryExtraEvent(match, match.EventDate.DateTime.AddMinutes(currentMatchTime + breakHalfTime + 3), 3);
            await SendEvent(eventMatch, baseUrl, eventPath);
            match = await matchRepository.GetMatchById(matchId);

            // Card Event
            eventMatch = MatchCardEventCreator.CreateYellowCardEvent(match, match.EventDate.DateTime.AddMinutes(currentMatchTime + breakHalfTime + 3), currentMatchTime, "away", "Player_" + rand.Next(999).ToString(), 2);
            await SendEvent(eventMatch, baseUrl, eventPath);
            match = await matchRepository.GetMatchById(matchId);

            // Break Awaiting Extra
            var awaitingExtra = 10;
            eventMatch = MatchTimeEventCreator.CreateAwaitingExtra(match, match.EventDate.DateTime.AddMinutes(currentMatchTime + breakHalfTime + extraTime));
            await SendEvent(eventMatch, baseUrl, eventPath);
            match = await matchRepository.GetMatchById(matchId);

            //First Extra Period Start
            eventMatch = MatchTimeEventCreator.CreateFirstExtraStart(match, match.EventDate.DateTime.AddMinutes(currentMatchTime + breakHalfTime + extraTime + awaitingExtra));
            await SendEvent(eventMatch, baseUrl, eventPath);
            match = await matchRepository.GetMatchById(matchId);

            // Card Event
            eventMatch = MatchCardEventCreator.CreateYellowCardEvent(match, match.EventDate.DateTime.AddMinutes(currentMatchTime + breakHalfTime + 3), currentMatchTime, "away", "Player_" + rand.Next(999).ToString(), 2);
            await SendEvent(eventMatch, baseUrl, eventPath);
            match = await matchRepository.GetMatchById(matchId);

            // Break Awaiting Extra
            currentMatchTime = 105;
            var breakExtraTime = 5;
            eventMatch = MatchTimeEventCreator.CreateBreakExtraStart(match, match.EventDate.DateTime.AddMinutes(currentMatchTime + breakHalfTime + extraTime));
            await SendEvent(eventMatch, baseUrl, eventPath);
            match = await matchRepository.GetMatchById(matchId);

            // Break Awaiting Extra
            eventMatch = MatchTimeEventCreator.CreateSecondExtraStart(match, match.EventDate.DateTime.AddMinutes(currentMatchTime + breakHalfTime + extraTime + breakExtraTime));
            await SendEvent(eventMatch, baseUrl, eventPath);
            match = await matchRepository.GetMatchById(matchId);

            // Score event
            currentMatchTime = 115;
            eventMatch = MatchScoreEventCreator.CreateNormalScore(match, match.EventDate.DateTime.AddMinutes(currentMatchTime + breakHalfTime + extraTime + breakExtraTime), currentMatchTime, "home", "Player_" + rand.Next(999).ToString(), false);
            await SendEvent(eventMatch, baseUrl, eventPath);
            match = await matchRepository.GetMatchById(matchId);

            // Await Penalty
            currentMatchTime = 120;
            int awaitPenalty = 10;
            eventMatch = MatchTimeEventCreator.CreateAwaitingPenalty(match, match.EventDate.DateTime.AddMinutes(currentMatchTime + breakHalfTime + extraTime + breakExtraTime));
            await SendEvent(eventMatch, baseUrl, eventPath);
            match = await matchRepository.GetMatchById(matchId);

            //Start Penalty
            eventMatch = MatchTimeEventCreator.CreatePenaltyStart(match, match.EventDate.DateTime.AddMinutes(currentMatchTime + breakHalfTime + extraTime + breakExtraTime + awaitPenalty));
            await SendEvent(eventMatch, baseUrl, eventPath);
            match = await matchRepository.GetMatchById(matchId);

            // Pen - 1 - Home - scored
            currentMatchTime = 122;
            eventMatch = MatchPenaltyEventCreator.CreatePenaltyEvent(match, match.EventDate.DateTime.AddMinutes(currentMatchTime + breakHalfTime + extraTime + breakExtraTime + awaitPenalty), "home", "Player_" + rand.Next(999).ToString(), "scored");
            await SendEvent(eventMatch, baseUrl, eventPath);
            match = await matchRepository.GetMatchById(matchId);

            // Pen - 1 - Away - scored
            currentMatchTime = 123;
            eventMatch = MatchPenaltyEventCreator.CreatePenaltyEvent(match, match.EventDate.DateTime.AddMinutes(currentMatchTime + breakHalfTime + extraTime + breakExtraTime + awaitPenalty), "away", "Player_" + rand.Next(999).ToString(), "scored");
            await SendEvent(eventMatch, baseUrl, eventPath);
            match = await matchRepository.GetMatchById(matchId);

            // Pen - 2 - Home - missed
            currentMatchTime = 124;
            eventMatch = MatchPenaltyEventCreator.CreatePenaltyEvent(match, match.EventDate.DateTime.AddMinutes(currentMatchTime + breakHalfTime + extraTime + breakExtraTime + awaitPenalty), "home", "Player_" + rand.Next(999).ToString(), "missed");
            await SendEvent(eventMatch, baseUrl, eventPath);
            match = await matchRepository.GetMatchById(matchId);

            // Pen - 2 - Away - scored
            currentMatchTime = 125;
            eventMatch = MatchPenaltyEventCreator.CreatePenaltyEvent(match, match.EventDate.DateTime.AddMinutes(currentMatchTime + breakHalfTime + extraTime + breakExtraTime + awaitPenalty), "away", "Player_" + rand.Next(999).ToString(), "scored");
            await SendEvent(eventMatch, baseUrl, eventPath);
            match = await matchRepository.GetMatchById(matchId);

            // Pen - 3 - Home - scored
            currentMatchTime = 126;
            eventMatch = MatchPenaltyEventCreator.CreatePenaltyEvent(match, match.EventDate.DateTime.AddMinutes(currentMatchTime + breakHalfTime + extraTime + breakExtraTime + awaitPenalty), "home", "Player_" + rand.Next(999).ToString(), "scored");
            await SendEvent(eventMatch, baseUrl, eventPath);
            match = await matchRepository.GetMatchById(matchId);

            // Pen - 3 - Away - missed
            currentMatchTime = 127;
            eventMatch = MatchPenaltyEventCreator.CreatePenaltyEvent(match, match.EventDate.DateTime.AddMinutes(currentMatchTime + breakHalfTime + extraTime + breakExtraTime + awaitPenalty), "away", "Player_" + rand.Next(999).ToString(), "missed");
            await SendEvent(eventMatch, baseUrl, eventPath);
            match = await matchRepository.GetMatchById(matchId);

            // Pen - 4 - Home - scored
            currentMatchTime = 128;
            eventMatch = MatchPenaltyEventCreator.CreatePenaltyEvent(match, match.EventDate.DateTime.AddMinutes(currentMatchTime + breakHalfTime + extraTime + breakExtraTime + awaitPenalty), "home", "Player_" + rand.Next(999).ToString(), "scored");
            await SendEvent(eventMatch, baseUrl, eventPath);
            match = await matchRepository.GetMatchById(matchId);

            // Pen - 4 - Away - scored
            currentMatchTime = 129;
            eventMatch = MatchPenaltyEventCreator.CreatePenaltyEvent(match, match.EventDate.DateTime.AddMinutes(currentMatchTime + breakHalfTime + extraTime + breakExtraTime + awaitPenalty), "away", "Player_" + rand.Next(999).ToString(), "scored");
            await SendEvent(eventMatch, baseUrl, eventPath);
            match = await matchRepository.GetMatchById(matchId);

            // Pen - 5 - Home - scored
            currentMatchTime = 130;
            eventMatch = MatchPenaltyEventCreator.CreatePenaltyEvent(match, match.EventDate.DateTime.AddMinutes(currentMatchTime + breakHalfTime + extraTime + breakExtraTime + awaitPenalty), "home", "Player_" + rand.Next(999).ToString(), "scored");
            await SendEvent(eventMatch, baseUrl, eventPath);
            match = await matchRepository.GetMatchById(matchId);

            // Pen - 5 - Away - missed
            currentMatchTime = 131;
            eventMatch = MatchPenaltyEventCreator.CreatePenaltyEvent(match, match.EventDate.DateTime.AddMinutes(currentMatchTime + breakHalfTime + extraTime + breakExtraTime + awaitPenalty), "away", "Player_" + rand.Next(999).ToString(), "missed");
            await SendEvent(eventMatch, baseUrl, eventPath);
            match = await matchRepository.GetMatchById(matchId);

            // Match end penalty
            currentMatchTime = 132;
            eventMatch = MatchTimeEventCreator.CreateMatchEndPenalty(match, match.EventDate.DateTime.AddMinutes(currentMatchTime + breakHalfTime + extraTime + breakExtraTime + awaitPenalty));
            await SendEvent(eventMatch, baseUrl, eventPath);

        }

        public static async Task<HttpResponseMessage> SendEvent(MatchPushEventDto matchEvent, string baseUrl, string eventPath)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(baseUrl);
                var result = await client.PostAsJsonAsync(eventPath, matchEvent);
                await Task.Delay(2000);

                return result;
            }
        }
    }
}
