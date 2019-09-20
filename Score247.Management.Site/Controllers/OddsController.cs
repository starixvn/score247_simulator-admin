namespace Score247.Management.Site.Controllers
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
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
            var viewModel = await oddsService.CreateOddsViewModel(id, GetTimezone());

            return View(viewModel);
        }

        [HttpPost]
        public async Task<ActionResult> GetOddsMovement([FromBody] OddsMovementRequestModel oddsMovementRequestModel)
        {
            var matchOddsMovement = await oddsService.GetMatchOddsMovement(oddsMovementRequestModel.MatchId, oddsMovementRequestModel.BetType, oddsMovementRequestModel.BookmakerId, "en-US", "des");

            return PartialView("1X2OddsMovement", matchOddsMovement);
        }

        [HttpPost]
        public async Task<JsonResult> SendOddsMovement([FromBody] OddsChange1X2RequestModel oddsChange1X2)
        {
            await oddsService.Send1X2OddsEvent(oddsChange1X2, GetTimezone());

            return Json(true);
        }

        private TimeSpan GetTimezone()
        {
            var result = HttpContext.Session.GetString("Timezone");
            if (string.IsNullOrEmpty(result))
            {
                result = "07:00:00";
            }

            TimeSpan.TryParse(result, out TimeSpan offset);

            return offset;
        }
    }
}