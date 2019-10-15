namespace Score247Management.Core.Matches.Models
{
    using MessagePack;

    namespace Soccer.Core.Matches.Models
    {
        [MessagePackObject(keyAsPropertyName: true)]
        public class Coverage
        {
            public Coverage(bool live, string trackerWidgetLink, bool basicScore = false, bool keyEvents = false, bool detailedEvents = false, bool lineups = false, bool commentary = false)
            {
                Live = live;
                BasicScore = basicScore;
                KeyEvents = keyEvents;
                DetailedEvents = detailedEvents;
                Lineups = lineups;
                Commentary = commentary;
                TrackerWidgetLink = trackerWidgetLink;
            }

            public bool Live { get; set; }

            public bool BasicScore { get; set; }

            public bool KeyEvents { get; set; }

            public bool DetailedEvents { get; set; }

            public bool Lineups { get; set; }

            public bool Commentary { get; set; }

            public string TrackerWidgetLink { get; set; }
        }
    }
}