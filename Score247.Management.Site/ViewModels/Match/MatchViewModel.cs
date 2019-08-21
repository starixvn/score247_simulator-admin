using System.Collections.Generic;
using Score247.Management.Site.Models;

namespace Score247.Management.Site.ViewModels.Match
{
    public class MatchViewModel
    {
        public IEnumerable<Score247Management.Core.Matches.Models.Match> Matches { get; set; }
        public DateSearchModel DateSearchModel { get; set; }
    }
}
