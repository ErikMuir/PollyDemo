using Polly;
using Polly.Timeout;
using PollyDemo.Common;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

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
            Console.WriteLine("Demo 2 - Fallback Policy");

            Logger.LogRequest(ActionType.Sending, HttpMethod.Get, Constants.FailRequest);

            var fallbackPolicy = Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                .Or<TimeoutRejectedException>()
                .FallbackAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("0"),
                });

            var response = await fallbackPolicy.ExecuteAsync(() => _httpClient.GetAsync(Constants.FailRequest));
            var content = await response.Content?.ReadAsStringAsync();

            Logger.LogResponse(ActionType.Received, response.StatusCode, content);
        }
    }
}
