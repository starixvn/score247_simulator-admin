using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Score247.Management.Site.Criterias.League;
using Score247.Management.Site.Helpers;
using Score247.Management.Site.Repositories.Leagues;
using Score247.Management.Site.Services;
using Score247.Management.Site.Services.Crawlers;
using Score247.Management.Site.ViewModels.League;

namespace Score247.Management.Site.Controllers
{
    public class LeagueController : Controller
    {
        private ILeagueService leagueService { get; }

        public LeagueController(ILeagueService leagueService)
        {
            this.leagueService = leagueService;
        }

        // GET: League
        public async Task<ActionResult> Index()
        {
            var repository = new LeagueRepository();
            var leagues = await repository.GetLeagues();
            return View(leagues);
        }

        // GET: League/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: League/Create
        public async Task<ActionResult> Create()
        {
            var repository = new LeagueRepository();
            var categories = await repository.GetLeagueCategory();
            var viewModel = new CreateLeagueViewModel();
            viewModel.LeagueCategories = categories;
            return View(viewModel);
        }

        // POST: League/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(IFormCollection collection)
        {
            try
            {
                var criteria = new CreateLeagueCriteria
                {
                    CategoryId = collection["League.LeagueCategory.Id"],
                    LeagueId = UniqueGenerator.GetIdWithPattern("sr:tournament:"),
                    LeagueName = collection["League.Name"],
                    LeagueOrder = Int32.Parse(collection["League.Order"])
                };

                var repository = new LeagueRepository();
                await repository.CreateLeague(criteria);
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        public ActionResult EnableOrDisableLeague(string id)
        {
            leagueService.EnableDisableLeague(id);

            return RedirectToAction(nameof(Index));
        }

        public ActionResult EnableMajorLeagues()

        {
            leagueService.EnableMajorLeague();

            return RedirectToAction(nameof(Index));
        }

        public ActionResult EnableAllLeagues()
        {
            leagueService.EnableAllLeague();

            return RedirectToAction(nameof(Index));
        }
    }
}