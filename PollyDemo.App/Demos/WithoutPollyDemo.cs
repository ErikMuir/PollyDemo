using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PollyDemo.Common;

namespace PollyDemo.App.Demos
{
    public class WithoutPollyDemo : IDemo
    {
        private HttpClient _httpClient;

        public WithoutPollyDemo(HttpClient client)
        {
            _httpClient = client;
        }

        public async Task Run()
        {
            Console.Clear();
            Console.WriteLine("Demo 1 - Without Polly");
            Console.ReadKey(true);

            DemoLogger.LogRequest(ActionType.Send, HttpMethod.Get, Constants.FailEndpoint);

            var response = await _httpClient.GetAsync(Constants.FailEndpoint);
            var content = JsonConvert.DeserializeObject<string>(await response.Content?.ReadAsStringAsync());

            DemoLogger.LogResponse(ActionType.Receive, response.StatusCode, content);
        }
    }
}
