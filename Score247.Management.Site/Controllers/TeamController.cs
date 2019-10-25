using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Score247.Management.Site.Criterias.Team;
using Score247.Management.Site.Helpers;
using Score247.Management.Site.Models;
using Score247.Management.Site.Repositories.Teams;

namespace Score247.Management.Site.Controllers
{
    public class TeamController : Controller
    {
        // GET: Team
        public async Task<ActionResult> Index()
        {
            var repository = new TeamRepository();
            var teams = await repository.GetTeams();
            return View(teams);
        }

        // GET: Team/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Team/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(IFormCollection collection)
        {
            try
            {
                var repository = new TeamRepository();
                var criteria = new CreateTeamCriteria
                {
                    Id = UniqueGenerator.GetIdWithPattern("sr:team:"),
                    TeamName = collection["Name"],
                    Abbreviation = collection["Abbreviation"],
                    Country = collection["Country"],
                    CountryCode = collection["CountryCode"]
                };
                await repository.CreateTeam(criteria);
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}