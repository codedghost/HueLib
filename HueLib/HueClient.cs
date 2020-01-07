using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using HueLib.Helper;
using Newtonsoft.Json;

namespace HueLib
{
    public class HueClient
    {
        private HttpClient _hueClient;
        private string _username;

        public HueClient(string ip, string username)
        {
            _hueClient = new HttpClient
            {
                BaseAddress = new Uri($"http://{ip}")
            };

            _username = username;
        }

        public async Task<bool> TurnOffAllLights()
        {
            var allLightsResult = await _hueClient.GetAsync($"/api/{_username}/lights");

            var resultString = await allLightsResult.Content.ReadAsStringAsync();
            var lightsDictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(resultString);

            foreach (var light in lightsDictionary)
            {
                var turnOffLightResult =
                    await _hueClient.PutAsync($"/api/{_username}/lights/{light.Key}/state", HttpClientHelper.GetJsonData(new {on = false}));
            }

            return true;
        }

        public async Task<bool> TurnOnAllLights()
        {
            var allLightsResult = await _hueClient.GetAsync($"/api/{_username}/lights");

            var resultString = await allLightsResult.Content.ReadAsStringAsync();
            var lightsDictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(resultString);

            foreach (var light in lightsDictionary)
            {
                var turnOffLightResult =
                    await _hueClient.PutAsync($"/api/{_username}/lights/{light.Key}/state", HttpClientHelper.GetJsonData(new { on = true }));
            }

            return true;
        }
    }
}