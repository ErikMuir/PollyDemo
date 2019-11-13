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
            const string endpoint = "/";

            #region -- Pre-Call Logging --
            await Clear();
            DemoLogger.LogRequest(ActionType.Send, endpoint);
            #endregion

            var response = await _httpClient.GetAsync(endpoint);
            var content = JsonConvert.DeserializeObject<string>(await response.Content?.ReadAsStringAsync());

            #region -- Post-Call Logging --
            DemoLogger.LogResponse(ActionType.Receive, response.StatusCode, content);
            #endregion
        }

        private async Task Clear()
        {
            Console.Clear();
            await _httpClient.GetAsync("/clear");
        }
    }
}
