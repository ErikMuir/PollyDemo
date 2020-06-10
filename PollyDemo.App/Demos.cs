using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using MuirDev.ConsoleTools;
using Polly;
using Polly.Bulkhead;
using Polly.CircuitBreaker;
using Polly.Extensions.Http;
using Polly.Timeout;

namespace PollyDemo.App
{
    public class Demos
    {
        public async Task<HttpResponseMessage> RetryWithoutPolly(string endpoint = "/fail/1")
        {
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

            return response;
        }

        public async Task<HttpResponseMessage> Retry(string endpoint = "/fail/1")
        {
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

            return await policy.ExecuteAsync(() => _httpClient.GetAsync(endpoint));
        }

        public async Task<HttpResponseMessage> HttpRequestException(string endpoint = "/")
        {
            var httpClient = new HttpClient { BaseAddress = new Uri("http://localhost:4444/api/WeatherForecast") };

            var policy = Policy
                .Handle<HttpRequestException>()
                .RetryAsync(onRetry: (ex, attempt) =>
                {
                    LogException(ex);
                    httpClient = _httpClient;
                });

            return await policy.ExecuteAsync(() => httpClient.GetAsync(endpoint));
        }

        public async Task<HttpResponseMessage> Delegates(string endpoint = "/auth")
        {
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

            return await policy.ExecuteAsync(() => _httpClient.GetAsync(endpoint));
        }

        public async Task<HttpResponseMessage> Timeout(string endpoint = "/timeout")
        {
            var policy = Policy.TimeoutAsync(3);

            HttpResponseMessage response = null;
            try
            {
                response = await policy.ExecuteAsync(async ct =>
                    await _httpClient.GetAsync(endpoint, ct),
                    CancellationToken.None);
            }
            catch (TimeoutRejectedException e)
            {
                LogException(e);
                return null;
            }

            return response;
        }

        public async Task<HttpResponseMessage> Fallback(string endpoint = "/fail")
        {
            var fallbackValue = "Same as today";
            var fallbackJson = JsonSerializer.Serialize(fallbackValue);
            var fallbackResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(fallbackJson)
            };

            var policy = HttpPolicyExtensions
                .HandleTransientHttpError()
                .FallbackAsync(fallbackResponse);

            return await policy.ExecuteAsync(() => _httpClient.GetAsync(endpoint));
        }

        public async Task<HttpResponseMessage> PolicyWrap(string endpoint = "/timeout/3")
        {
            var retryPolicy = HttpPolicyExtensions
                .HandleTransientHttpError()
                .Or<TimeoutRejectedException>() // thrown by Polly's TimeoutPolicy if the inner execution times out
                .RetryAsync(3);

            var timeoutPolicy = Policy.TimeoutAsync<HttpResponseMessage>(3);

            var wrappedPolicy = Policy.WrapAsync(retryPolicy, timeoutPolicy);

            HttpResponseMessage response = null;

            try
            {
                response = await wrappedPolicy.ExecuteAsync(async ct =>
                    await _httpClient.GetAsync(endpoint, ct),
                    CancellationToken.None);
            }
            catch (TimeoutRejectedException) { }

            return response;
        }

        public async Task<HttpResponseMessage> CircuitBreaker(string endpoint = "/fail")
        {
            var policy = HttpPolicyExtensions
                .HandleTransientHttpError()
                .CircuitBreakerAsync(
                    handledEventsAllowedBeforeBreaking: 3,
                    durationOfBreak: TimeSpan.FromSeconds(10),
                    onBreak: (exception, timespan) => _console.Failure("The circuit is now open and is not allowing calls for the next 10 seconds.", _noEOL),
                    onReset: () => _console.Success("The circuit is now closed and all requests will be allowed."),
                    onHalfOpen: () => _console.LineFeed().Warning("The circuit is now half-open and will allow one request.")
                );

            HttpResponseMessage response = null;

            while (true)
            {
                try
                {
                    response = await policy.ExecuteAsync(() => _httpClient.GetAsync(endpoint));
                    if (response.IsSuccessStatusCode) break;
                }
                catch (BrokenCircuitException)
                {
                    endpoint = happyPathEndpoint;
                    HandleException(++_exceptionCount);
                }
            }

            return response;
        }

        public async Task<HttpResponseMessage> AdvancedCircuitBreaker(string endpoint = "/fail")
        {
            var policy = HttpPolicyExtensions
                .HandleTransientHttpError()
                .AdvancedCircuitBreakerAsync(
                    failureThreshold: 0.5, // Break on >=50% actions result in handled exceptions...
                    samplingDuration: TimeSpan.FromSeconds(10), // ... over any 10 second period
                    minimumThroughput: 8, // ... provided at least 8 actions in the 10 second period.
                    durationOfBreak: TimeSpan.FromSeconds(10), // Break for 10 seconds.
                    onBreak: (exception, timespan) => _console.Failure("The circuit is now open and is not allowing calls for the next 10 seconds.", _noEOL),
                    onReset: () => _console.Success("The circuit is now closed and all requests will be allowed."),
                    onHalfOpen: () => _console.LineFeed().Warning("The circuit is now half-open and will allow one request.")
                );

            HttpResponseMessage response = null;

            while (true)
            {
                try
                {
                    response = await policy.ExecuteAsync(() => _httpClient.GetAsync(endpoint));
                    if (response.IsSuccessStatusCode) break;
                }
                catch (BrokenCircuitException)
                {
                    endpoint = happyPathEndpoint;
                    HandleException(++_exceptionCount);
                }
            }

            return response;
        }

        public async Task<HttpResponseMessage> BulkheadIsolation(string endpoint = "/timeout")
        {
            HttpResponseMessage response = null;

            try
            {
                _console.Info($"Bulkhead available count: {_bulkheadPolicy.BulkheadAvailableCount}");
                _console.Info($"Queue available count: {_bulkheadPolicy.QueueAvailableCount}");
                response = await _bulkheadPolicy.ExecuteAsync(() => _httpClient.GetAsync(endpoint));
            }
            catch (BulkheadRejectedException) { }

            return response;
        }

        public void ConfigureInStartup()
        {
            var retryPolicy = HttpPolicyExtensions
                .HandleTransientHttpError()
                .Or<TimeoutRejectedException>() // thrown by Polly's TimeoutPolicy if the inner execution times out
                .RetryAsync(3);

            var timeoutPolicy = Policy.TimeoutAsync<HttpResponseMessage>(10);

            var services = new ServiceCollection();

            services
                .AddHttpClient<App>(x =>
                {
                    x.BaseAddress = new Uri("http://localhost:5000/api/WeatherForecast");
                    x.DefaultRequestHeaders.Accept.Clear();
                    x.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                })
                .AddPolicyHandler(retryPolicy)
                .AddPolicyHandler(timeoutPolicy);
        }

        #region [Demo Orchestration]
        private readonly HttpClient _httpClient;
        private readonly AsyncBulkheadPolicy _bulkheadPolicy = Policy.BulkheadAsync(4, 2);
        public Demos(HttpClient client)
        {
            _httpClient = client;
        }
        private const string happyPathEndpoint = "/";
        private static readonly FluentConsole _console = new FluentConsole();
        private static readonly LogOptions _noEOL = new LogOptions(false);
        private static void LogException(Exception exception) { }
        private static void HandleException(int exceptionCount) { }
        private static int _exceptionCount = 0;
        #endregion
    }
}
