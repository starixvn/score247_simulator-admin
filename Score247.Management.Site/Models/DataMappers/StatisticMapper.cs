namespace Score247.Management.Site.Models.DataMappers
{
    using Score247.Management.Site.Models.Events;
    using Score247Management.Core.Teams.Models;

    public static class StatisticMapper
    {
        public static Statistics1 MapStatistic(TeamStatistic statistic)
        {
            var statistics = new Statistics1()
            {
                shots_blocked = statistic.ShotsBlocked,
                fouls = statistic.Fouls,
                yellow_cards = statistic.YellowCards,
                throw_ins = statistic.ThrowIns,
                corner_kicks = statistic.CornerKicks,
                ball_possession = statistic.Possession,
                free_kicks = statistic.FreeKicks,
                goal_kicks = statistic.GoalKicks,
                shots_off_target = statistic.ShotsOffTarget,
                offsides = statistic.Offsides,
                shots_saved = statistic.ShotsSaved,
                injuries = statistic.Injuries,
                shots_on_target = statistic.ShotsOnTarget,
                red_cards = statistic.RedCards
            };

            return statistics;
        }
    }
}