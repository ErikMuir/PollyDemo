using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PollyDemo.Common;

namespace PollyDemo.App.Demos
{
    public class WithoutPolly : IDemo
    {
        private HttpClient _httpClient;

        public WithoutPolly(HttpClient client)
        {
            _httpClient = client;
        }

        public async Task Run()
        {
            Console.Clear();
            Console.WriteLine("Demo 1 - Without Polly");
            Console.ReadKey(true);

            DemoLogger.LogRequest(ActionType.Send, "/fail");

            var response = await _httpClient.GetAsync("/fail");
            var content = JsonConvert.DeserializeObject<string>(await response.Content?.ReadAsStringAsync());

            DemoLogger.LogResponse(ActionType.Receive, response.StatusCode, content);
        }
    }
}
