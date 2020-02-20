using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Score247.Management.Site.Services.Images;
using System;
using System.Threading.Tasks;

namespace Score247.Management.Site.Controllers
{
    public class ImagesController : Controller
    {
        private ISportTeamImageService sportTeamImageService;

        public ImagesController(ISportTeamImageService sportTeamImageService)
        {
            this.sportTeamImageService = sportTeamImageService;
        }

        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> GetFlags(IFormCollection collection)
        {
            try
            {
                await sportTeamImageService.SaveImageToFile();
            }
            catch (Exception ex)
            {
            }

            return View(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> GetTeams(IFormCollection collection)
        {
            try
            {
                await sportTeamImageService.GetTeamsFromLeagues();
            }
            catch (Exception ex)
            {
            }

            return View(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> GetSportRadarTeamsLogo(IFormCollection collection)
        {
            try
            {
                await sportTeamImageService.GetSportRadarTeamsLogo();
            }
            catch (Exception ex)
            {
            }

            return View(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CrawlSportRadarTeamsLogo(IFormCollection collection)
        {
            try
            {
                await sportTeamImageService.CrawlSportRadarTeamsLogo();
            }
            catch (Exception ex)
            {
            }

            return View(nameof(Index));
        }
    }
}