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
    public class CircuitBreakerRecoversDemo : IDemo
    {
        private HttpClient _httpClient;

        public CircuitBreakerRecoversDemo(HttpClient client)
        {
            _httpClient = client;
        }

        public async Task Run()
        {
            Console.WriteLine("Demo 9 - Circuit Breaker Policy (recovers)");

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
                    Logger.LogRequest(ActionType.Sending, HttpMethod.Get, Constants.IrregularRequest);

                    response = await policy.ExecuteAsync(() => _httpClient.GetAsync(Constants.IrregularRequest));
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
