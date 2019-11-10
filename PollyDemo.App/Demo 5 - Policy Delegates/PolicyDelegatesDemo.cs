using Polly;
using Polly.Retry;
using PollyDemo.Common;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace PollyDemo.App.Demos
{
    public class PolicyDelegatesDemo : IDemo
    {
        private HttpClient _httpClient;

        public PolicyDelegatesDemo(HttpClient client)
        {
            _httpClient = client;
        }

        public async Task Run()
        {
            Console.WriteLine("Demo 5 - Policy Delegates");

            var expiredToken = new AuthenticationHeaderValue("Bearer", "expired-token");
            var freshToken = new AuthenticationHeaderValue("Bearer", "fresh-token");

            _httpClient.DefaultRequestHeaders.Authorization = expiredToken;

            Logger.LogRequest(ActionType.Sending, HttpMethod.Get, Constants.AuthRequest);

            var httpRetryPolicy =
                Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                    .RetryAsync(3, onRetry: (httpResponseMessage, i) =>
                    {
                        if (httpResponseMessage.Result.StatusCode == HttpStatusCode.Unauthorized)
                        {
                            Console.WriteLine("Refreshing auth token ...");
                            _httpClient.DefaultRequestHeaders.Authorization = freshToken;
                        }
                    });

            var response = await httpRetryPolicy.ExecuteAsync(() => _httpClient.GetAsync(Constants.AuthRequest));
            var content = await response.Content?.ReadAsStringAsync();

            Logger.LogResponse(ActionType.Received, response.StatusCode, content);
        }
    }
}
