using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using HueLib.Helper;
using HueLib.RequestModels;
using HueLib.ResponseModels;
using HueLib.ResponseModels.ChildModels;
using Newtonsoft.Json;

namespace HueLib
{
    public class GetUsernameHueClient
    {
        private HttpClient _hueClient;

        public GetUsernameHueClient(string ip)
        {
            _hueClient = new HttpClient
            {
                BaseAddress = new Uri($"http://{ip}")
            };
        }

        public async Task<string> GetUsername(string applicationName, string deviceName)
        {
            var result = await _hueClient.PostAsync("/api", HttpClientHelper.GetJsonData(new GetUsernameRequest
            {
                devicetype = $"{applicationName}#{deviceName}"
            }));

            var jsonResult = await result.Content.ReadAsStringAsync();

            var response = JsonConvert.DeserializeObject<UsernameSuccess>(jsonResult);

            if (response == null || string.IsNullOrWhiteSpace(response.username)) return string.Empty;

            return response.username;
        }
    }
}