using System;
using System.Collections.Generic;
using System.Linq;
using Fanex.Data;
using Fanex.Data.MySql;
using Fanex.Data.Repository;
using Fanex.Logging;
using Fanex.Logging.Sentry.RavenClient;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Quartz.Impl;
using Quartz.Spi;
using Score247.Management.Site.ScheduledTask;
using Score247.Management.Site.Services;
using Score247.Management.Site.Services.Matches;
using Score247.Management.Site.Services.Odds;
using Score247.Management.Site.Services.Schedulers;
using SharpRaven;

namespace Score247.Management.Site
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDistributedMemoryCache();

            services.AddSession(options =>
            {
                options.Cookie.Name = ".Score247.Session";
                options.IdleTimeout = TimeSpan.FromSeconds(3600);
                options.Cookie.IsEssential = true;
            });
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddSingleton(Configuration);
            services.AddSingleton<IDynamicRepository, DynamicRepository>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.AddMvc()
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            RegisterServices(services);

            RegisterScheduledJobs(services);
        }

        private void RegisterServices(IServiceCollection services)
        {
            services.AddSingleton<ILeagueService, LeagueService>();
            services.AddSingleton<IOddsService, OddsService>();
            services.AddSingleton<ITriggerJobService, TriggerJobService>();
            services.AddSingleton<ISimulatorService, SimulatorService>();
            services.AddSingleton<IMatchService, MatchService>();
            services.AddSingleton<IQueryService, QueryService>();
            services.AddSingleton<ICreateMatchesService, CreateMatchesService>();
            services.AddSingleton<IMatchEventsService, MatchEventsService>();
            services.AddSingleton<IScenarioService, ScenarioService>();
            services.AddSingleton<IAutoEventService, AutoEventService>();
            services.AddSingleton<ITimelineService, TimelineService>();
        }

        private void RegisterScheduledJobs(IServiceCollection services)
        {
            // Add Quartz services
            services.AddSingleton<IJobFactory, SingletonJobFactory>();
            services.AddSingleton<ISchedulerFactory, StdSchedulerFactory>();

            // Add our job
            services.AddSingleton<CleanTimelinesJob>();
            services.AddSingleton(new JobSchedule(
                jobType: typeof(CleanTimelinesJob),
                cronExpression: "0 0 0 * * ?")); // run every 5 seconds

            services.AddHostedService<QuartzHostedService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            string baseDir = env.ContentRootPath;
            AppDomain.CurrentDomain.SetData("DataDirectory", System.IO.Path.Combine(baseDir, "App_Data"));

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            LogManager
             .SetDefaultLogCategory(Configuration.GetValue<string>("Sentry:DefaultCategory"))
             .Use(new List<ILogEngine>
             {
                 new SentryLogging(new SentryEngineOptions
                 {
                     Dsn = new Dsn(Configuration.GetValue<string>("Sentry:Url")),
                     FrontEndDsn = new Dsn(Configuration.GetValue<string>("Sentry:Url"))
                 })
             });

            var connections = Configuration
                 .GetSection("ConnectionStrings")
                 .GetChildren()
                 .ToDictionary(connection => connection.Key,
                               connection => new ConnectionConfiguration(connection.Key, connection.Value));

            // Set up data directory
            DbSettingProviderManager
               .StartNewSession()
               .Use(connections)
               // It comes with a default connection string provider, which works well with MySql connections,
               // as well as a default DbSetting provider
               .WithMySql(resourcePath: AppDomain.CurrentDomain.GetData("DataDirectory").ToString())
               //.UseDefaultConnectionStringProvider()
               .Run();

            app.UseStaticFiles();
            app.UseCookiePolicy();
            app.UseSession();
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Match}/{action=Index}/{id?}");
            });
        }
    }
}