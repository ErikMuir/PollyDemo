using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Polly;
using Polly.Timeout;
using PollyDemo.Common;

namespace PollyDemo.App.Demos
{
    public class Fallback : IDemo
    {
        private HttpClient _httpClient;

        public Fallback(HttpClient client)
        {
            _httpClient = client;
        }

        public async Task Run()
        {
            Console.Clear();
            Console.WriteLine("Demo 2 - Fallback Policy");
            Console.ReadKey(true);

            DemoLogger.LogRequest(ActionType.Send, "/fail");

            var fallbackValue = "Unknown";
            var serializedFallbackValue = JsonConvert.SerializeObject(fallbackValue);

            var fallbackPolicy = Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                .Or<TimeoutRejectedException>()
                .FallbackAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(serializedFallbackValue),
                });

            var response = await fallbackPolicy.ExecuteAsync(() => _httpClient.GetAsync("/fail"));
            var content = JsonConvert.DeserializeObject<string>(await response.Content?.ReadAsStringAsync());

            DemoLogger.LogResponse(ActionType.Receive, response.StatusCode, content);
        }
    }
}
