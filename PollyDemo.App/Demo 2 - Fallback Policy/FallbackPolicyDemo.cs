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
    public class FallbackPolicyDemo : IDemo
    {
        private HttpClient _httpClient;

        public FallbackPolicyDemo(HttpClient client)
        {
            _httpClient = client;
        }

        public async Task Run()
        {
            Console.Clear();
            Console.WriteLine("Demo 2 - Fallback Policy");
            Console.ReadKey(true);

            Logger.LogRequest(ActionType.Sending, HttpMethod.Get, Constants.FailEndpoint);

            var fallbackValue = "Unknown";
            var serializedFallbackValue = JsonConvert.SerializeObject(fallbackValue);

            var fallbackPolicy = Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                .Or<TimeoutRejectedException>()
                .FallbackAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(serializedFallbackValue),
                });

            var response = await fallbackPolicy.ExecuteAsync(() => _httpClient.GetAsync(Constants.FailEndpoint));
            var content = JsonConvert.DeserializeObject<string>(await response.Content?.ReadAsStringAsync());

            Logger.LogResponse(ActionType.Received, response.StatusCode, content);
        }
    }
}
