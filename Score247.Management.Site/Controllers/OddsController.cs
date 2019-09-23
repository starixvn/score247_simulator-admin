namespace Score247.Management.Site.Controllers
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Score247.Management.Site.Helpers;
    using Score247.Management.Site.Services.Odds;
    using Score247.Management.Site.ViewModels.Odds;

    public class OddsController : Controller
    {
        private IOddsService oddsService { get; }

        public OddsController(IOddsService oddsService)
        {
            this.oddsService = oddsService;
        }

        public async Task<ActionResult> Index(string id)
        {
            var viewModel = await oddsService.CreateOddsViewModel(id, DateTimeHelper.GetTimezone(HttpContext.Session));

            return View(viewModel);
        }

        [HttpPost]
        public async Task<ActionResult> GetOddsMovement([FromBody] OddsMovementRequestModel oddsMovementRequestModel)
        {
            var matchOddsMovement = await oddsService.GetMatchOddsMovement(oddsMovementRequestModel.MatchId, oddsMovementRequestModel.BetType, oddsMovementRequestModel.BookmakerId, "en-US", "des");

            return PartialView(oddsMovementRequestModel.BetName + "OddsMovement", matchOddsMovement);
        }

        [HttpPost]
        public async Task<ActionResult> GetOddsComparison([FromBody] OddsComparisonRequestModel oddsComparisonRequest)
        {
            var matchOdds = await oddsService.GetMatchOdds(oddsComparisonRequest.MatchId, oddsComparisonRequest.BetType, "en-US", "des");

            return PartialView(oddsComparisonRequest.BetName + "OddsComparison", matchOdds);
        }

        [HttpPost]
        public async Task<JsonResult> SendOddsMovement([FromBody] OddsChangeRequestModel oddsChangeRequestModel)
        {
            await oddsService.SendOddsEvent(oddsChangeRequestModel, DateTimeHelper.GetTimezone(HttpContext.Session));

            return Json(true);
        }
    }
}