using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Polly;
using Polly.Timeout;

namespace PollyDemo.App
{
    public class Timeout : IDemo
    {
        private HttpClient _httpClient;

        public Timeout(HttpClient client)
        {
            _httpClient = client;
        }

        public async Task Run()
        {
            Console.Clear();
            Console.WriteLine("Demo 6 - Timeout Policy");
            Console.ReadKey(true);

            DemoLogger.LogRequest(ActionType.Send, "/timeout");

            var timeoutPolicy = Policy.TimeoutAsync(5, TimeoutStrategy.Optimistic);

            try
            {
                var response = await timeoutPolicy.ExecuteAsync(async token =>
                    await _httpClient.GetAsync("/timeout", token),
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
