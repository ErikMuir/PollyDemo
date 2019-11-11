using Newtonsoft.Json;
using Polly;
using Polly.Timeout;
using PollyDemo.Common;
using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace PollyDemo.App.Demos
{
    public class PolicyWrappingDemo : IDemo
    {
        private HttpClient _httpClient;

        public PolicyWrappingDemo(HttpClient client)
        {
            _httpClient = client;
        }

        public async Task Run()
        {
            Console.Clear();
            Console.WriteLine("Demo 7 - Policy Wrapping");
            Console.ReadKey(true);

            Logger.LogRequest(ActionType.Sending, HttpMethod.Get, Constants.SlowEndpoint);

            var timeoutPolicy = Policy.TimeoutAsync(2);

            var retryPolicy = Policy
                .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                .Or<TimeoutRejectedException>()
                .RetryAsync(3);

            var fallbackValue = "Unknown";
            var serializedFallbackValue = JsonConvert.SerializeObject(fallbackValue);

            var fallbackPolicy = Policy
                .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                .Or<TimeoutRejectedException>()
                .FallbackAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(serializedFallbackValue),
                });

            // var response = 
            //     await 
            //     fallbackPolicy.ExecuteAsync(() =>
            //        retryPolicy.ExecuteAsync(() =>
            //            timeoutPolicy.ExecuteAsync(async token =>
            //                await _httpClient.GetAsync(Constants.SlowRequest, token),
            //                CancellationToken.None)));

            var wrappedPolicy = Policy.WrapAsync(fallbackPolicy, retryPolicy).WrapAsync(timeoutPolicy);

            var response = await wrappedPolicy.ExecuteAsync(async token =>
                await _httpClient.GetAsync(Constants.SlowEndpoint, token),
                CancellationToken.None);
            var content = JsonConvert.DeserializeObject<string>(await response.Content?.ReadAsStringAsync());

            Logger.LogResponse(ActionType.Received, response.StatusCode, content);
        }
    }
}
