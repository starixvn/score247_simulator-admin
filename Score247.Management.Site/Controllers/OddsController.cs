using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Score247.Management.Site.Repositories.Matches;
using Score247.Management.Site.Services.Odds;
using Score247.Management.Site.ViewModels.Matches;
using Score247.Management.Site.ViewModels.Odds;

namespace Score247.Management.Site.Controllers
{
    public class OddsController : Controller
    {
        private IOddsService oddsService { get; }

        public OddsController(IOddsService oddsService)
        {
            this.oddsService = oddsService;
        }

        public async Task<ActionResult> Index(string id)
        {
            TimeSpan offset;
            TimeSpan.TryParse(GetTimezone(), out offset);

            var viewModel = await oddsService.CreateOddsViewModel(id, offset);

            return View(viewModel);
        }

        [HttpPost]
        public async Task<ActionResult> GetOddsMovement([FromBody] OddsMovementRequestModel oddsMovementRequestModel)
        {
            var matchOddsMovement = await oddsService.GetMatchOddsMovement(oddsMovementRequestModel.MatchId, oddsMovementRequestModel.BetType, oddsMovementRequestModel.BookmakerId, "en-US", "des");

            return PartialView("1x2OddsMovement", matchOddsMovement);
        }

        private string GetTimezone()
        {
            var result = HttpContext.Session.GetString("Timezone");
            if (string.IsNullOrEmpty(result))
            {
                result = "07:00:00";
            }
            return result;
        }
    }
}