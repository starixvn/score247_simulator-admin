using Score247.Management.Site.Services;

namespace Score247.Management.Site.ScheduledTask
{
    using System.Threading.Tasks;
    using Quartz;

    public class CleanTimelinesJob : IJob
    {
        private readonly ISimulatorService simulatorService;

        public CleanTimelinesJob(ISimulatorService simulatorService)
        {
            this.simulatorService = simulatorService;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            await simulatorService.SendCleanTimelines();
        }
    }
}