using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PollyDemo.Common;

namespace PollyDemo.App
{
    public class AppClient
    {
        private readonly HttpClient _httpClient;

        public AppClient(HttpClient client)
        {
            _httpClient = client;
        }

        public async Task Run()
        {
            await Clear();

            var endpoint = "/fail";

            Logger.LogRequest(ActionType.Send, endpoint);

            var response = await _httpClient.GetAsync(endpoint);

            var content = JsonConvert.DeserializeObject<string>(await response.Content?.ReadAsStringAsync());

            Logger.LogResponse(ActionType.Receive, response.StatusCode, content);
        }

        private async Task Clear()
        {
            Console.Clear();
            await _httpClient.GetAsync("/clear");
        }
    }
}
