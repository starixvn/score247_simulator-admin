using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Score247.Management.Site.Models.Events;
using Score247.Management.Site.Services;

namespace Score247.Management.Site.Controllers
{
    public class LiveController : Controller
    {
        private readonly IMatchService matchService;

        public LiveController(IMatchService matchService)
        {
            this.matchService = matchService;
        }

        public async Task<IActionResult> PreviewLiveMatch(string matchId)
        {
            var result = await matchService.GetSimulatorResult(matchId);

            if (result == null)
            {
            }
            return PartialView("LiveMatchSimulatorPartial", result);
        }

        [HttpPost("Live/UpdateLiveMatch")]
        public async Task<IActionResult> UpdateLiveMatch([FromQuery] string matchId, [FromBody] Sport_Event_Status sport_Event_Status)
        {
            await matchService.SendCustomLiveMatch(matchId, sport_Event_Status);

            await Task.Delay(2000);

            return Json(true);
        }
    }
}