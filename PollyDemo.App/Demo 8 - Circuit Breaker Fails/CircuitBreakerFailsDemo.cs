using Polly;
using Polly.CircuitBreaker;
using Polly.Wrap;
using PollyDemo.Common;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace PollyDemo.App.Demos
{
    public class CircuitBreakerFailsDemo : IDemo
    {
        private HttpClient _httpClient;

        public CircuitBreakerFailsDemo(HttpClient client)
        {
            _httpClient = client;

        }

        public async Task Run()
        {
            Console.WriteLine("Demo 8 - Circuit Breaker Policy (fails)");

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
                    Logger.LogRequest(ActionType.Sending, HttpMethod.Get, Constants.FailRequest);

                    response = await policy.ExecuteAsync(() => _httpClient.GetAsync(Constants.FailRequest));
                    var content = await response.Content?.ReadAsStringAsync();

                    Logger.LogResponse(ActionType.Received, response.StatusCode, content);
                }
                while (!response.IsSuccessStatusCode);
            }
            catch (BrokenCircuitException e)
            {
                Logger.LogException(e);
            }
        }
    }
}
