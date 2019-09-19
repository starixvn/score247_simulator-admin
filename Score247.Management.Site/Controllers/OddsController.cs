using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Score247.Management.Site.Repositories.Matches;
using Score247.Management.Site.Services.Odds;
using Score247.Management.Site.ViewModels.Matches;

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