using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Score247Management.Core.Leagues.Models;
using Score247.Management.Site.Criterias.League;
using Score247.Management.Site.Helpers;
using Score247.Management.Site.Repositories.League;
using Score247.Management.Site.ViewModels.League;

namespace Score247.Management.Site.Controllers
{
    public class LeagueController : Controller
    {
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
                // TODO: Add insert logic here
                var criteria = new CreateLeagueCriteria();
                criteria.CategoryId = collection["League.LeagueCategory.Id"];
                criteria.LeagueId = UniqueGenerator.GetIdWithPattern("sr:tournament:");
                criteria.LeagueName = collection["League.Name"];
                criteria.LeagueOrder = Int32.Parse(collection["League.Order"]);

                var repository = new LeagueRepository();
                await repository.CreateLeague(criteria);
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: League/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: League/Edit/5
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

        // GET: League/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: League/Delete/5
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