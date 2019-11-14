using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
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
            Console.Clear();

            while (true)
            {
                const string endpoint = "/";

                #region "Demo Orchestration - Log Request"
                Console.Write("\nPress any key...");
                Console.ReadKey(true);
                await Clear();
                DemoLogger.LogRequest(ActionType.Send, endpoint);
                #endregion


                var response = await _httpClient.GetAsync(endpoint);
                var content = JsonConvert.DeserializeObject<string>(await response.Content?.ReadAsStringAsync());


                #region "Demo Orchestration - Log Response"
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
