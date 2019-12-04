using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fanex.Data;
using Fanex.Data.MySql;
using Fanex.Data.Repository;
using Fanex.Logging;
using Fanex.Logging.Sentry.RavenClient;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
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
        public Startup(IConfiguration configuration, IHostingEnvironment environment)
        {
            Configuration = configuration;
            Environment = environment;
        }

        public IConfiguration Configuration { get; }
        public IHostingEnvironment Environment { get; }

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

            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
              .AddCookie(options =>
              {
                  ////options.Cookie.HttpOnly = true;
                  //options.Cookie.SecurePolicy = Environment.IsDevelopment()
                  //  ? CookieSecurePolicy.None : CookieSecurePolicy.Always;
                  options.Cookie.SameSite = SameSiteMode.Lax;
                  options.Cookie.Name = "Score247.AuthCookieAdmin";
                  options.LoginPath = "/Login";
              });

            //services.Configure<CookiePolicyOptions>(options =>
            //{
            //    options.MinimumSameSitePolicy = SameSiteMode.Strict;
            //    options.HttpOnly = HttpOnlyPolicy.None;
            //    options.Secure = Environment.IsDevelopment()
            //      ? CookieSecurePolicy.None : CookieSecurePolicy.Always;
            //});

            services.AddSingleton(Configuration);
            services.AddSingleton<IDynamicRepository, DynamicRepository>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.AddMvc(options => options.Filters.Add(new AuthorizeFilter()))
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            RegisterServices(services);

            RegisterScheduledJobs(services);

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

            services.AddSingleton(Logger.Log);
        }

        private static void RegisterServices(IServiceCollection services)
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
            services.AddSingleton<ILineupService, LineupService>();
            services.AddSingleton<ISimulatorFileService, SimulatorFileService>();
            services.AddSingleton<ICreateMatchesService, CreateMatchesService>();
            services.AddSingleton<IH2HService, H2HService>();
            services.AddSingleton<ITeamService, TeamService>();
            services.AddSingleton<ILoginService, LoginService>();
        }

        private static void RegisterScheduledJobs(IServiceCollection services)
        {
            // Add Quartz services
            services.AddSingleton<IJobFactory, SingletonJobFactory>();
            services.AddSingleton<ISchedulerFactory, StdSchedulerFactory>();

            services.AddHostedService<QuartzHostedService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IAntiforgery antiforgery)
        {
            string baseDir = env.ContentRootPath;
            AppDomain.CurrentDomain.SetData("DataDirectory", System.IO.Path.Combine(baseDir, "App_Data"));

            app.UseDeveloperExceptionPage();

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
            app.UseAuthentication();

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