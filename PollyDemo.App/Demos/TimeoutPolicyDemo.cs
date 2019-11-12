using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Polly;
using Polly.Timeout;
using PollyDemo.Common;

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

            DemoLogger.LogRequest(ActionType.Send, "/slow");

            var timeoutPolicy = Policy.TimeoutAsync(5, TimeoutStrategy.Optimistic);

            try
            {
                var response = await timeoutPolicy.ExecuteAsync(async token =>
                    await _httpClient.GetAsync("/slow", token),
                    CancellationToken.None);
                var content = JsonConvert.DeserializeObject<string>(await response.Content?.ReadAsStringAsync());

                DemoLogger.LogResponse(ActionType.Receive, response.StatusCode, content);
            }
            catch (TimeoutRejectedException e)
            {
                DemoLogger.LogException(e);
            }
        }
    }
}
