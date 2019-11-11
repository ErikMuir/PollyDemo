using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Polly;
using PollyDemo.Common;

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
            Console.Clear();
            Console.WriteLine("Demo 5 - Policy Delegates");
            Console.ReadKey(true);

            var expiredToken = new AuthenticationHeaderValue("Bearer", "expired-token");
            var freshToken = new AuthenticationHeaderValue("Bearer", "fresh-token");

            _httpClient.DefaultRequestHeaders.Authorization = expiredToken;

            Logger.LogRequest(ActionType.Sending, HttpMethod.Get, Constants.AuthEndpoint);

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

            var response = await httpRetryPolicy.ExecuteAsync(() => _httpClient.GetAsync(Constants.AuthEndpoint));
            var content = JsonConvert.DeserializeObject<string>(await response.Content?.ReadAsStringAsync());

            Logger.LogResponse(ActionType.Received, response.StatusCode, content);
        }
    }
}
