using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using MuirDev.ConsoleTools;
using Newtonsoft.Json;
using Polly;
using Polly.CircuitBreaker;
using Polly.Extensions.Http;
using Polly.Timeout;

namespace PollyDemo.App
{
    public class Demos
    {
        private readonly HttpClient _httpClient;
        private static readonly FluentConsole _console = new FluentConsole();
        private static readonly LogOptions _noEOL = new LogOptions(false);
        private static readonly LogOptions _endpoint = new LogOptions(ConsoleColor.DarkYellow);
        private static readonly LogOptions _success = new LogOptions(ConsoleColor.DarkGreen);
        private static readonly LogOptions _failure = new LogOptions(ConsoleColor.DarkRed);
        private static readonly LogOptions _forecast = new LogOptions(ConsoleColor.DarkCyan);
        private static int _exceptionCount;
        private static void LogException(Exception exception) { }
        private static void LogResponse(HttpResponseMessage response) { }
        private static void HandleException() { }

        public Demos(HttpClient client)
        {
            _httpClient = client;
        }

        public async void RetryWithoutPolly()
        {
            const string endpoint = "/fail/1";

            HttpResponseMessage response = null;
            const int retryLimit = 3;
            for (int attempt = 0; attempt <= retryLimit; attempt++)
            {
                response = await _httpClient.GetAsync(endpoint);
                if (response.IsSuccessStatusCode)
                {
                    break;
                }
            }
        }

        public async void Retry()
        {
            const string endpoint = "/fail/1";

            // handle failures
            var policy = Policy
                .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                .RetryAsync();

            // don't handle bad requests
            policy = Policy
                .HandleResult<HttpResponseMessage>(r => (int)r.StatusCode >= 500)
                .RetryAsync();

            // handle request timeouts
            policy = Policy
                .HandleResult<HttpResponseMessage>(r => (int)r.StatusCode >= 500 || r.StatusCode == HttpStatusCode.RequestTimeout)
                .RetryAsync();

            // handle network failures
            policy = Policy
                .HandleResult<HttpResponseMessage>(r => (int)r.StatusCode >= 500 || r.StatusCode == HttpStatusCode.RequestTimeout)
                .Or<HttpRequestException>()
                .RetryAsync();

            // handle all/only transient http errors
            policy = HttpPolicyExtensions
                .HandleTransientHttpError()
                .RetryAsync();

            // problems may last more than an instant
            policy = HttpPolicyExtensions
                .HandleTransientHttpError()
                .RetryAsync(3);

            // leave some space to give it a chance to recover
            policy = HttpPolicyExtensions
                .HandleTransientHttpError()
                .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromMilliseconds(500));

            // progressively back off
            policy = HttpPolicyExtensions
                .HandleTransientHttpError()
                .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt) / 2));

            var response = await policy.ExecuteAsync(() => _httpClient.GetAsync(endpoint));
        }

        public async void HttpRequestException()
        {
            const string endpoint = "/";

            var httpClient = new HttpClient { BaseAddress = new Uri("http://localhost:4444/api/WeatherForecast") };

            var policy = Policy
                .Handle<HttpRequestException>()
                .RetryAsync(onRetry: (ex, attempt) =>
                {
                    LogException(ex);
                    httpClient = _httpClient;
                });

            var response = await policy.ExecuteAsync(() => httpClient.GetAsync(endpoint));
        }

        public async void Delegates()
        {
            const string endpoint = "/auth";

            var expiredToken = new AuthenticationHeaderValue("Bearer", "expired-token");
            var freshToken = new AuthenticationHeaderValue("Bearer", "fresh-token");

            _httpClient.DefaultRequestHeaders.Authorization = expiredToken;

            var policy = Policy
                .HandleResult<HttpResponseMessage>(r => r.StatusCode == HttpStatusCode.Unauthorized)
                .RetryAsync(onRetry: (response, retryCount) =>
                {
                    Console.WriteLine("Refreshing auth token...");
                    Task.Delay(1000).Wait(); // simulate refreshing auth token
                    _httpClient.DefaultRequestHeaders.Authorization = freshToken;
                });

            var response = await policy.ExecuteAsync(() => _httpClient.GetAsync(endpoint));
        }

        public async Task Timeout()
        {
            var endpoint = "/timeout";

            var policy = Policy.TimeoutAsync(3);

            try
            {
                var response = await policy.ExecuteAsync(async ct =>
                    await _httpClient.GetAsync(endpoint, ct),
                    CancellationToken.None);

                LogResponse(response);
            }
            catch (TimeoutRejectedException e)
            {
                LogException(e);
            }

            // var policy = Policy.TimeoutAsync(3, onTimeoutAsync: async (context, timespan, task) =>
            // {
            //     _console.Warning($"{context.PolicyKey}: execution timed out after {timespan.TotalSeconds} seconds.");
            //     await Task.CompletedTask;
            // });
        }

        public async void Fallback()
        {
            var endpoint = "/fail";

            var fallbackValue = "Same as today";
            var fallbackJson = JsonConvert.SerializeObject(fallbackValue);
            var fallbackResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(fallbackJson)
            };

            var policy = HttpPolicyExtensions
                .HandleTransientHttpError()
                .FallbackAsync(fallbackResponse);

            var response = await policy.ExecuteAsync(() => _httpClient.GetAsync(endpoint));
        }

        public void ConfigureInStartup()
        {
            var services = new ServiceCollection();

            var retryPolicy = HttpPolicyExtensions
                .HandleTransientHttpError()
                .Or<TimeoutRejectedException>() // thrown by Polly's TimeoutPolicy if the inner execution times out
                .RetryAsync(3);

            var timeoutPolicy = Policy.TimeoutAsync<HttpResponseMessage>(10);


            services
                .AddHttpClient<App>(x =>
                {
                    x.BaseAddress = new Uri("http://localhost:5000/api/WeatherForecast");
                    x.DefaultRequestHeaders.Accept.Clear();
                    x.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                })
                .AddPolicyHandler(retryPolicy)
                .AddPolicyHandler(timeoutPolicy);

            // var wrappedPolicy = retryPolicy.WrapAsync(timeoutPolicy).WrapAsync(retryPolicy);

            // services
            //     .AddHttpClient<App>(x =>
            //     {
            //         x.BaseAddress = new Uri("http://localhost:5000/api/WeatherForecast");
            //         x.DefaultRequestHeaders.Accept.Clear();
            //         x.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            //     })
            //     .AddPolicyHandler(wrappedPolicy);
        }

        public async Task CircuitBreaker()
        {
            var endpoint = "fail";

            var retry = HttpPolicyExtensions
                .HandleTransientHttpError()
                .RetryForeverAsync();

            var breaker = HttpPolicyExtensions
                .HandleTransientHttpError()
                .CircuitBreakerAsync(
                    handledEventsAllowedBeforeBreaking: 3,
                    durationOfBreak: TimeSpan.FromSeconds(10),
                    onBreak: (exception, timespan) => _console.Failure("The circuit is now open and is not allowing calls for the next 10 seconds.", _noEOL),
                    onReset: () => _console.Success("The circuit is now closed and all requests will be allowed."),
                    onHalfOpen: () => _console.LineFeed().Warning("The circuit is now half-open and will allow one request."));

            var policy = Policy.WrapAsync(retry, breaker);

            while (true)
            {
                try
                {
                    var response = await policy.ExecuteAsync(() => _httpClient.GetAsync(endpoint));

                    LogResponse(response);

                    break;
                }
                catch (BrokenCircuitException)
                {
                    endpoint = "/";
                    HandleException();
                }
            }
        }
    }
}
