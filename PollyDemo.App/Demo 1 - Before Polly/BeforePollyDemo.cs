using Newtonsoft.Json;
using PollyDemo.Common;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace PollyDemo.App.Demos
{
    public class BeforePollyDemo : IDemo
    {
        private HttpClient _httpClient;

        public BeforePollyDemo(HttpClient client)
        {
            _httpClient = client;
        }

        public async Task Run()
        {
            Console.Clear();
            Console.WriteLine("Demo 1 - Without Polly");
            Console.ReadKey(true);

            Logger.LogRequest(ActionType.Sending, HttpMethod.Get, Constants.FailEndpoint);

            var response = await _httpClient.GetAsync(Constants.FailEndpoint);
            var content = JsonConvert.DeserializeObject<string>(await response.Content?.ReadAsStringAsync());

            Logger.LogResponse(ActionType.Received, response.StatusCode, content);
        }
    }
}
