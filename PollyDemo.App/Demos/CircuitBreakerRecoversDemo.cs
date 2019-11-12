using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Polly;
using Polly.CircuitBreaker;
using PollyDemo.Common;

namespace PollyDemo.App.Demos
{
    public class CircuitBreakerRecoversDemo : IDemo
    {
        private HttpClient _httpClient;

        public CircuitBreakerRecoversDemo(HttpClient client)
        {
            _httpClient = client;
        }

        public async Task Run()
        {
            Console.Clear();
            Console.WriteLine("Demo 9 - Circuit Breaker Policy (recovers)");
            Console.ReadKey(true);

            var retry = Policy
                .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                .RetryAsync(4);

            var breaker = Policy
                .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                .CircuitBreakerAsync(
                    handledEventsAllowedBeforeBreaking: 3,
                    durationOfBreak: TimeSpan.FromSeconds(15),
                    onBreak: (exception, timespan) =>
                    {
                        Console.WriteLine("Breaker was tripped!");
                    },
                    onReset: () => { });

            var policy = Policy.WrapAsync(retry, breaker);

            HttpResponseMessage response;

            try
            {
                do
                {
                    DemoLogger.LogRequest(ActionType.Send, "/irregular");

                    response = await policy.ExecuteAsync(() => _httpClient.GetAsync("/irregular"));
                    var content = JsonConvert.DeserializeObject<string>(await response.Content?.ReadAsStringAsync());

                    DemoLogger.LogResponse(ActionType.Receive, response.StatusCode, content);
                }
                while (!response.IsSuccessStatusCode);
            }
            catch (BrokenCircuitException e)
            {
                DemoLogger.LogException(e);
            }
        }
    }
}
