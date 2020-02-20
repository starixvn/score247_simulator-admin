using Fanex.Data.Repository;
using Newtonsoft.Json;
using Score247.Management.Site.Criterias.Images;
using Score247Management.Core.Images;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Score247.Management.Site.Services.Images
{
    public interface ISportTeamImageService
    {
        Task GetTeamsFromLeagues();

        Task SaveImageToFile();

        Task GetSportRadarTeamsLogo();

        Task CrawlSportRadarTeamsLogo();
    }

    public class SportTeamImageService : ISportTeamImageService
    {
        private const string RabitTeamByLeagueApi = "https://api-football-v1.p.rapidapi.com/v2/teams/league/{0}";
        private const string ApiUrl = "https://media.api-football.com/teams/{0}.png";
        private const string SportradarApi = "https://api.sportradar.us/soccer-images-p3/ap{0}?api_key={1}";

        private readonly string[] SportradarTeamsLogoApis = {
            "https://api.sportradar.us/soccer-images-p3/ap/bundesliga/logos/2019/manifest.json?api_key=485tzq8mq2zpdakc4mc3z9kr",
            "https://api.sportradar.us/soccer-images-p3/ap/epl/logos/2020/manifest.json?api_key=mhbyv22y38hbar77rhnats5g",
            "https://api.sportradar.us/soccer-images-p3/ap/ligue-1/logos/2019/manifest.json?api_key=su5tdfb3s87b8dg32a65c3mz",
            "https://api.sportradar.us/soccer-images-p3/ap/la-liga/logos/2019/manifest.json?api_key=ceyx2gg82xx93zgnuwam4a7e",
            "https://api.sportradar.us/soccer-images-p3/ap/serie-a/logos/2019/manifest.json?api_key=95q29cs9549fd8tb6gzwksjh"
        };

        private readonly IDictionary<string, string> SportRadarKey = new Dictionary<string, string>
        {
            ["bundesliga"] = "485tzq8mq2zpdakc4mc3z9kr",
            ["epl"] = "mhbyv22y38hbar77rhnats5g",
            ["ligue-1"] = "su5tdfb3s87b8dg32a65c3mz",
            ["la-liga"] = "ceyx2gg82xx93zgnuwam4a7e",
            ["serie-a"] = "95q29cs9549fd8tb6gzwksjh"
        };

        //private const string ApiUrl = "https://sportteamslogo.com/api?key={0}&size=big&tid={1}";

        private IDynamicRepository dynamicRepository;
        private IHttpClientFactory clientFactory;

        public SportTeamImageService(IDynamicRepository dynamicRepository, IHttpClientFactory clientFactory)
        {
            this.dynamicRepository = dynamicRepository;
            this.clientFactory = clientFactory;
        }

        public async Task GetSportRadarTeamsLogo()
        {
            foreach (var apiUrl in SportradarTeamsLogoApis)
            {
                try
                {
                    using (HttpClient client = clientFactory.CreateClient())
                    {
                        var request = new HttpRequestMessage(HttpMethod.Get, apiUrl);
                        request.Headers.Add("Accept", "application/json");
                        var response = await client.SendAsync(request);

                        if (response.IsSuccessStatusCode)
                        {
                            var responseStream = await response.Content.ReadAsStringAsync();
                            var result = JsonConvert.DeserializeObject<SportRadarTeamLogoResponse>(responseStream);
                            await InsertOrUpdateSportRadarTeams(result.Assetlist);
                        }
                        else
                        {
                        }
                    }
                }
                catch (Exception ex)
                {
                }
            }
        }

        private async Task InsertOrUpdateSportRadarTeams(IEnumerable<SportRadarTeamAsset> assetlist)
        {
            var teams = new List<SportRadarTeamLogo>();
            foreach (var asset in assetlist)
            {
                if (asset.TeamId != null && !asset.Title.Contains("(transparent)"))
                {
                    teams.Add(new SportRadarTeamLogo(asset.TeamId, asset.Title, null, asset.Links.FirstOrDefault(link => link.Type == "original")?.Href));
                }
            }

            await dynamicRepository.ExecuteAsync(new InsertSportradarTeamsLogoCommand(teams));
        }

        public async Task GetTeamsFromLeagues()
        {
            var leagueIds = await dynamicRepository.FetchAsync<int>(new GetUnProcessRabitLeaguesIdCriteria());
            foreach (var id in leagueIds)
            {
                try
                {
                    using (HttpClient client = clientFactory.CreateClient())
                    {
                        var url = string.Format(RabitTeamByLeagueApi, id);
                        var request = new HttpRequestMessage(HttpMethod.Get, url);
                        request.Headers.Add("Accept", "application/json");
                        request.Headers.Add("x-rapidapi-host", "api-football-v1.p.rapidapi.com");
                        request.Headers.Add("x-rapidapi-key", "e665f6f1e3msh9f6ba5f2e3c82dfp184267jsn5034a2863471");
                        var response = await client.SendAsync(request);

                        if (response.IsSuccessStatusCode)
                        {
                            var responseStream = await response.Content.ReadAsStringAsync();
                            var result = JsonConvert.DeserializeObject<RapidTeamByLeague>(responseStream);
                            await InsertOrUpdateTeams(result?.Api.teams);
                            await dynamicRepository.ExecuteAsync(new UpdateRabitLeagueStatusCommand(id, true));
                        }
                        else
                        {
                        }
                    }
                }
                catch (Exception ex)
                {
                }
            }
        }

        public async Task SaveImageToFile()
        {
            var teams = await dynamicRepository.FetchAsync<RapidTeam>(new GetUnProcessRabitTeamsCriteria());
            foreach (var team in teams)
            {
                try
                {
                    using (WebClient client = new WebClient())
                    {
                        var url = string.Format(ApiUrl, team.TeamId);
                        await client.DownloadFileTaskAsync(new Uri(url), $"d:\\XamarinForm\\score247-assets\\assets\\SportTeamsImages\\{team.Name}.png");
                    }
                }
                catch (Exception ex)
                {
                }
                await dynamicRepository.ExecuteAsync(new UpdateRabitTeamStatusCommand(team.TeamId, true));
            }
        }

        private async Task InsertOrUpdateTeams(IEnumerable<RapidTeam> teams)
        {
            await dynamicRepository.ExecuteAsync(new InsertRapidTeamsCommand(teams));
        }

        public async Task CrawlSportRadarTeamsLogo()
        {
            var teams = await dynamicRepository.FetchAsync<SportRadarTeamLogo>(new GetSportradarTeamsLogoUnFetchedCriteria());
            foreach (var team in teams)
            {
                try
                {
                    using (WebClient client = new WebClient())
                    {
                        var url = string.Format(SportradarApi, team.Href, GetSportradarKey(team.Href));
                        await client.DownloadFileTaskAsync(new Uri(url), $"d:\\SportRadarTeamsImages\\{team.Code}.png");
                    }
                }
                catch (Exception ex)
                {
                }
                await dynamicRepository.ExecuteAsync(new UpdateSportradarTeamsLogoStatusCommand(team.TeamId, true));
            }
        }

        private string GetSportradarKey(string href)
        {
            foreach (var key in SportRadarKey.Keys)
            {
                if (href.Contains(key))
                {
                    return SportRadarKey[key];
                }
            }
            return null;
        }
    }
}