namespace Score247Management.Core.Odds
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Score247Management.Core._Shared.Enumerations;
    using Score247Management.Core._Shared.Resources;
    using Score247Management.Core.Matches.Models;
    using Score247Management.Core.Odds.Models;

    public static class OddsMovementProcessor
    {
        private const int firstPeriod = 1;
        private const int secondPeriod = 2;
        private const int startSecondHaft = 46;
        private const string pause = "pause";

        public static IEnumerable<OddsMovement> BuildOddsMovements(
            Match match,
            List<BetTypeOdds> betTypeOddsList)
        {
            if (match == null)
            {
                return Enumerable.Empty<OddsMovement>();
            }

            var oddsMovements = BuildOddMovementBeforeMatchStart(betTypeOddsList, match)
                .Concat(BuildOddMovementAfterMatchStarted(betTypeOddsList, match));

            CalculateOddsTrend(oddsMovements);

            return oddsMovements.Reverse();
        }

        public static BetTypeOdds AssignOpeningOddsToFirstOdds(IGrouping<string, BetTypeOdds> group)
        {
            var orderedGroup = group.OrderByDescending(bto => bto.LastUpdatedTime);
            var first = orderedGroup.First();
            var last = orderedGroup.Last();

            first.AssignOpeningData(last.BetOptions);

            return first;
        }

        private static readonly IList<byte> OddChangeEventIds
            = new List<byte>
            {
                 EventType.PeriodStart.Value,
                 EventType.ScoreChange.Value,
                 EventType.MatchEnded.Value,
                 EventType.BreakStart.Value
            };

        private static IEnumerable<TimelineEvent> GetMainEvents(Match match)
            => match.TimeLines.Where(tl
#pragma warning disable S1067 // Expressions should not be too complex
                 => IsTimelineNeedMapWithOddsData(tl)).OrderBy(t => t.Time);

        public static bool IsTimelineNeedMapWithOddsData(TimelineEvent tl)
        {
            var needMap = tl != null
                                && tl.Type != null
                                && OddChangeEventIds.Contains(tl.Type.Value)
                                && tl.PeriodType != null
                                && (tl.PeriodType == PeriodType.RegularPeriod
                                        || tl.PeriodType.DisplayName == pause);

            return needMap;
        }

#pragma warning restore S1067 // Expressions should not be too complex

        private static void CalculateOddsTrend(IEnumerable<OddsMovement> oddsMovements)
        {
            OddsMovement prevOdds = null;
            foreach (var oddsMovement in oddsMovements)
            {
                if (prevOdds == null)
                {
                    prevOdds = oddsMovement;
                }
                else
                {
                    oddsMovement.CalculateOddsTrend(prevOdds.BetOptions);
                    prevOdds = oddsMovement;
                }
            }
        }

        private static IEnumerable<OddsMovement> BuildOddMovementAfterMatchStarted(
            IEnumerable<BetTypeOdds> betTypeOddsList,
            Match match)
        {
            if (match.TimeLines == null
                || !match.TimeLines.Any())
            {
                return Enumerable.Empty<OddsMovement>();
            }

            var homeScore = 0;
            var awayScore = 0;
            TimelineEvent currentEvent = null;
            var timelineEvents = GetMainEvents(match);
            var oddsMovements = new List<OddsMovement>();
            var matchLiveOdds = betTypeOddsList.Where(o => o.LastUpdatedTime >= match.EventDate);

            foreach (var betTypeOdds in matchLiveOdds)
            {
                var timelineEvent = timelineEvents.FirstOrDefault(e => e.Time == betTypeOdds.LastUpdatedTime);
                var oddsMovement = BuildOddsMovementEvent(ref homeScore, ref awayScore, ref currentEvent, timelineEvent, betTypeOdds);

                oddsMovements.Add(oddsMovement);
            }

            return oddsMovements;
        }

        private static OddsMovement BuildOddsMovementEvent(
            ref int homeScore,
            ref int awayScore,
            ref TimelineEvent currentEvent,
            TimelineEvent timelineEvent,
            BetTypeOdds betTypeOdds)
        {
            var matchTime = string.Empty;

            if (timelineEvent != null)
            {
                if (IsScoreChange(timelineEvent))
                {
                    homeScore = timelineEvent.HomeScore;
                    awayScore = timelineEvent.AwayScore;
                    matchTime = $"{timelineEvent.MatchTime}'";
                }

                if (IsHalfTimeBreakStart(timelineEvent))
                {
                    currentEvent = timelineEvent;
                    matchTime = AppResources.HT;
                }

                if (IsFirstPeriodStart(timelineEvent))
                {
                    currentEvent = timelineEvent;
                    matchTime = AppResources.KO;
                }

                if (IsSecondPeriodStart(timelineEvent))
                {
                    currentEvent = timelineEvent;
                    currentEvent.Time = currentEvent.Time.AddMinutes(-startSecondHaft);
                }
            }

            if (string.IsNullOrWhiteSpace(matchTime)
                && currentEvent != null)
            {
                if (IsHalfTimeBreakStart(currentEvent))
                {
                    matchTime = AppResources.HT;
                }
                else
                {
                    var totalMinutes = (betTypeOdds.LastUpdatedTime - currentEvent.Time).TotalMinutes;
                    matchTime = totalMinutes.ToString("0") + "'";
                }
            }

            return new OddsMovement(
                betTypeOdds.BetOptions,
                matchTime,
                betTypeOdds.LastUpdatedTime,
                currentEvent != null,
                homeScore,
                awayScore);
        }

        private static bool IsScoreChange(TimelineEvent timelineEvent)
            => timelineEvent != null
                && timelineEvent.Type == EventType.ScoreChange;

        private static bool IsHalfTimeBreakStart(TimelineEvent timelineEvent)
            => timelineEvent != null
                && timelineEvent.Type == EventType.BreakStart
                && timelineEvent.PeriodType.DisplayName.ToLowerInvariant() == pause;

        private static bool IsSecondPeriodStart(TimelineEvent timelineEvent)
            => timelineEvent != null
                && timelineEvent.Type == EventType.PeriodStart
                && timelineEvent.Period == secondPeriod
                && timelineEvent.PeriodType == PeriodType.RegularPeriod;

        private static bool IsFirstPeriodStart(TimelineEvent timelineEvent)
            => timelineEvent != null
                && timelineEvent.Type == EventType.PeriodStart
                && timelineEvent.Period == firstPeriod
                && timelineEvent.PeriodType == PeriodType.RegularPeriod;

        private static IEnumerable<OddsMovement> BuildOddMovementBeforeMatchStart(
            List<BetTypeOdds> betTypeOddsList,
            Match match)
        {
            var firstBetTypeOdds = betTypeOddsList.FirstOrDefault();
            var oddsMovements = new List<OddsMovement>
            {
                BuildOpeningOddsMovement(firstBetTypeOdds)
            }
            .Concat(BuildLiveOddsMovement(betTypeOddsList, match, firstBetTypeOdds));

            return oddsMovements;
        }

        private static IEnumerable<OddsMovement> BuildLiveOddsMovement(List<BetTypeOdds> betTypeOddsList, Match match, BetTypeOdds firstBetTypeOdds)
        {
            var liveOddsMovement = new List<OddsMovement>();
            var beforeLiveMatchOdds = betTypeOddsList.Where(
                            o => o.LastUpdatedTime < match.EventDate
                                && o.LastUpdatedTime > firstBetTypeOdds.LastUpdatedTime);

            foreach (var betTypeOdds in beforeLiveMatchOdds)
            {
                var oddsMovement = new OddsMovement(betTypeOdds.BetOptions, AppResources.Live, betTypeOdds.LastUpdatedTime);

                liveOddsMovement.Add(oddsMovement);
            }

            return liveOddsMovement;
        }

        private static OddsMovement BuildOpeningOddsMovement(BetTypeOdds firstBetTypeOdds)
        {
            var openingOddsMovement = new OddsMovement(firstBetTypeOdds.BetOptions, AppResources.Opening, firstBetTypeOdds.LastUpdatedTime);

            openingOddsMovement.ResetLiveOddsToOpeningOdds();

            return openingOddsMovement;
        }
    }
}