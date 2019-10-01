namespace Score247.Management.Site.Controllers
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Score247.Management.Site.Services;

    public class ScenariosController : Controller
    {
        private readonly IScenarioService scenarioService;

        public ScenariosController(IScenarioService scenarioService)
        {
            this.scenarioService = scenarioService;
        }

        public async Task<IActionResult> Index()
        {
            var scenarios = await scenarioService.GetAllScenarios();

            return View(scenarios);
        }
    }
}