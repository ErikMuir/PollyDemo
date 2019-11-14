using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Polly;
using Polly.CircuitBreaker;
using PollyDemo.Common;

namespace PollyDemo.App
{
    public class CircuitBreakerRecovers : IDemo
    {
        private HttpClient _httpClient;

        public CircuitBreakerRecovers(HttpClient client)
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
                    DemoLogger.LogRequest(ActionType.Send, "/fail/4");

                    response = await policy.ExecuteAsync(() => _httpClient.GetAsync("/fail/4"));
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
