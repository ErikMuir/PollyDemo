using Newtonsoft.Json;
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
        private readonly PolicyWrap<HttpResponseMessage> _policy;

        public CircuitBreakerFailsDemo()
        {
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

            _policy = Policy.WrapAsync(retry, breaker);
        }

        public async Task Run()
        {
            Console.WriteLine("Demo 8 - Circuit Breaker Policy (fails)");

            _httpClient = GetHttpClient();

            HttpResponseMessage response;

            try
            {
                do
                {
                    Logger.LogRequest(ActionType.Sending, HttpMethod.Get, Constants.FailRequest);

                    response = await _policy.ExecuteAsync(() => _httpClient.GetAsync(Constants.FailRequest));
                    var content = null as object;

                    if (response.IsSuccessStatusCode)
                        content = JsonConvert.DeserializeObject<int>(await response.Content.ReadAsStringAsync());
                    else if (response.Content != null)
                        content = await response.Content.ReadAsStringAsync();
                    Logger.LogResponse(ActionType.Received, response.StatusCode, content);
                }
                while (!response.IsSuccessStatusCode);
            }
            catch (BrokenCircuitException e)
            {
                Logger.LogException(e);
            }
        }

        private HttpClient GetHttpClient()
        {
            var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(Constants.BaseAddress);
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            return httpClient;
        }
    }
}
