using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Score247Management.Criterias.Team;
using Score247Management.Helpers;
using Score247Management.Repositories.Team;

namespace Score247Management.Controllers
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

        // GET: Team/Details/5
        public ActionResult Details(int id)
        {
            return View();
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
                // TODO: Add insert logic here
                var repository = new TeamRepository();
                var criteria = new CreateTeamCriteria();
                criteria.Id = UniqueGenerator.GetIdWithPattern("sr:team:");
                criteria.TeamName = collection["Name"];
                criteria.Abbreviation = collection["Abbreviation"];
                criteria.Country = collection["Country"];
                criteria.CountryCode = collection["CountryCode"];
                await repository.CreateTeam(criteria);
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: Team/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: Team/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: Team/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: Team/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}