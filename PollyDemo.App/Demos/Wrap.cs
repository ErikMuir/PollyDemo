using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Polly;
using Polly.Timeout;
using PollyDemo.Common;

namespace PollyDemo.App.Demos
{
    public class Wrap : IDemo
    {
        private HttpClient _httpClient;

        public Wrap(HttpClient client)
        {
            _httpClient = client;
        }

        public async Task Run()
        {
            Console.Clear();
            Console.WriteLine("Demo 7 - Policy Wrapping");
            Console.ReadKey(true);

            DemoLogger.LogRequest(ActionType.Send, "/timeout");

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
            //                await _httpClient.GetAsync("/timeout", token),
            //                CancellationToken.None)));

            var wrappedPolicy = Policy.WrapAsync(fallbackPolicy, retryPolicy).WrapAsync(timeoutPolicy);

            var response = await wrappedPolicy.ExecuteAsync(async token =>
                await _httpClient.GetAsync("/timeout", token),
                CancellationToken.None);
            var content = JsonConvert.DeserializeObject<string>(await response.Content?.ReadAsStringAsync());

            DemoLogger.LogResponse(ActionType.Receive, response.StatusCode, content);
        }
    }
}
