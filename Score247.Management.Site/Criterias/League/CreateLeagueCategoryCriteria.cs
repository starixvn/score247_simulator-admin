using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fanex.Data.Repository;

namespace Score247.Management.Site.Criterias.League
{
    public class CreateLeagueCategoryCriteria : NonQueryCommand
    {
        public string CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string CategoryCode { get; set; }

        public override string GetSettingKey()
            => "League_CreateLeagueCategory";

        public override bool IsValid()
            => true;
    }
}