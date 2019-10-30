namespace Score247.Management.Site.Controllers
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Score247.Management.Site.Criterias.Team;
    using Score247.Management.Site.Helpers;
    using Score247.Management.Site.Models;
    using Score247.Management.Site.Repositories.Teams;
    using Score247.Management.Site.Services;

    public class TeamController : Controller
    {
        private readonly IMatchService matchService;
        private readonly ITeamService teamService;

        public TeamController(IMatchService matchService, ITeamService teamService)
        {
            this.matchService = matchService;
            this.teamService = teamService;
        }

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

        // Result

        [HttpPost("/Team/Results/Generate")]
        public async Task<ActionResult> GenerateTeamResult([FromQuery]  string matchId, [FromQuery] string teamQualifier, [FromQuery] string teamResultCase)
        {
            var h2HDto = await teamService.GenerateTeamResult(matchId, teamQualifier, teamResultCase);

            return Json(h2HDto);
        }

        [HttpPost("/Team/Results/Apply/{matchId}")]
        public async Task<ActionResult> ApplyTeamResult(string matchId, [FromBody] TeamResultDto teamResultDto)
        {
            await teamService.ApplyTeamResult(teamResultDto);

            return Json(true);
        }
    }
}