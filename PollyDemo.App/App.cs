using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Polly;
using Polly.Extensions.Http;
using PollyDemo.Common;

namespace PollyDemo.App
{
    public class App
    {
        private readonly HttpClient _httpClient;

        public App(HttpClient client)
        {
            _httpClient = client;
        }

        public async Task Run()
        {
            while (true)
            {
                const string endpoint = "/";

                #region "Demo Orchestration - Log Request"
                Console.WriteLine("\nPress any key...");
                Console.ReadKey(true);
                await Clear();
                DemoLogger.LogRequest(ActionType.Send, endpoint);
                await Task.Delay(250);
                #endregion


                var response = await _httpClient.GetAsync(endpoint);


                #region "Demo Orchestration - Log Response"
                var content = JsonConvert.DeserializeObject<string>(await response.Content?.ReadAsStringAsync());
                DemoLogger.LogResponse(ActionType.Receive, response.StatusCode, content);
                #endregion
            }
        }

        #region "Demo Orchestration"
        private async Task Clear()
        {
            Console.Clear();
            await _httpClient.GetAsync("/clear");
        }
        #endregion
    }
}
