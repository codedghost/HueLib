using System;
using System.Net.Http;
using System.Threading.Tasks;
using HueLib.Helper;
using HueLib.RequestModels;

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

        public async Task<string> GetUsername()
        {
            var result = await _hueClient.PostAsync("/api", HttpClientHelper.GetJsonData(new GetUsernameRequest
            {
                devicetype = "my_hue_app#iphone sean"
            }));

            var jsonResult = await result.Content.ReadAsStringAsync();

            return jsonResult;
        }
    }
}