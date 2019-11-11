using Newtonsoft.Json;
using Polly;
using Polly.Timeout;
using PollyDemo.Common;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace PollyDemo.App.Demos
{
    public class TimeoutPolicyDemo : IDemo
    {
        private HttpClient _httpClient;

        public TimeoutPolicyDemo(HttpClient client)
        {
            _httpClient = client;
        }

        public async Task Run()
        {
            Console.Clear();
            Console.WriteLine("Demo 6 - Timeout Policy");
            Console.ReadKey(true);

            Logger.LogRequest(ActionType.Sending, HttpMethod.Get, Constants.SlowEndpoint);

            var timeoutPolicy = Policy.TimeoutAsync(5, TimeoutStrategy.Optimistic);

            try
            {
                var response = await timeoutPolicy.ExecuteAsync(async token =>
                    await _httpClient.GetAsync(Constants.SlowEndpoint, token),
                    CancellationToken.None);
                var content = JsonConvert.DeserializeObject<string>(await response.Content?.ReadAsStringAsync());

                Logger.LogResponse(ActionType.Received, response.StatusCode, content);
            }
            catch (TimeoutRejectedException e)
            {
                Logger.LogException(e);
            }
        }
    }
}
