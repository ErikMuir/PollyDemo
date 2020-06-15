using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Bulkhead;
using Polly.CircuitBreaker;
using Polly.Extensions.Http;
using Polly.Timeout;

namespace PollyDemo.App
{
    public partial class Demos
    {
        public async Task<HttpResponseMessage> RetryWithoutPolly(string path = "/fail/1")
        {
            HttpResponseMessage response = null;

            const int retryLimit = 3;

            for (int attempt = 0; attempt <= retryLimit; attempt++)
            {
                response = await _httpClient.GetAsync(path);
                if (response.IsSuccessStatusCode)
                {
                    break;
                }
            }

            return response;
        }

        public async Task<HttpResponseMessage> Retry(string path = "/fail/1")
        {
            // handle failures
            // fail 1
            var policy = Policy
                .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                .RetryAsync();

            return await policy.ExecuteAsync(() => _httpClient.GetAsync(path));

            // don't handle bad requests
            // bad-request
            policy = Policy
                .HandleResult<HttpResponseMessage>(r => (int)r.StatusCode >= 500)
                .RetryAsync();

            // handle request timeouts
            // timeout 1
            policy = Policy
                .HandleResult<HttpResponseMessage>(r => (int)r.StatusCode >= 500 || r.StatusCode == HttpStatusCode.RequestTimeout)
                .RetryAsync();

            // handle network failures
            // no demo
            policy = Policy
                .HandleResult<HttpResponseMessage>(r => (int)r.StatusCode >= 500 || r.StatusCode == HttpStatusCode.RequestTimeout)
                .Or<HttpRequestException>()
                .RetryAsync();

            // handle all/only transient http errors
            // fail 1
            // bad-request
            // timeout 1
            policy = HttpPolicyExtensions
                .HandleTransientHttpError()
                .RetryAsync();

            // problems may last more than an instant
            // fail 3
            policy = HttpPolicyExtensions
                .HandleTransientHttpError()
                .RetryAsync(3);

            // leave some space to give it a chance to recover
            // fail 3
            policy = HttpPolicyExtensions
                .HandleTransientHttpError()
                .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromMilliseconds(500));

            // progressively back off
            // fail 3
            policy = HttpPolicyExtensions
                .HandleTransientHttpError()
                .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt) / 2));
        }

        public async Task<HttpResponseMessage> HttpRequestException(string path = "/")
        {
            var httpClient = new HttpClient { BaseAddress = new Uri("http://localhost:4444/api/WeatherForecast") };

            var policy = Policy
                .Handle<HttpRequestException>()
                .RetryAsync(onRetry: (ex, attempt) =>
                {
                    _logger.LogException(ex);
                    httpClient = _httpClient;
                });

            return await policy.ExecuteAsync(() => httpClient.GetAsync(path));
        }

        public async Task<HttpResponseMessage> Delegates(string path = "/auth")
        {
            var expiredToken = new AuthenticationHeaderValue("Bearer", "expired-token");

            _httpClient.DefaultRequestHeaders.Authorization = expiredToken;

            var policy = Policy
                .HandleResult<HttpResponseMessage>(r => r.StatusCode == HttpStatusCode.Unauthorized)
                .RetryAsync(onRetry: (response, retryCount) =>
                {
                    Console.WriteLine("Refreshing auth token...");

                    Task.Delay(1000).Wait(); // simulate refreshing auth token
                    var freshToken = new AuthenticationHeaderValue("Bearer", "fresh-token");
                    
                    _httpClient.DefaultRequestHeaders.Authorization = freshToken;
                });

            return await policy.ExecuteAsync(() => _httpClient.GetAsync(path));
        }

        public async Task<HttpResponseMessage> Timeout(string path = "/timeout")
        {
            var policy = Policy.TimeoutAsync(3);

            HttpResponseMessage response;
            try
            {
                response = await policy.ExecuteAsync(async ct =>
                    await _httpClient.GetAsync(path, ct),
                    CancellationToken.None);
            }
            catch (TimeoutRejectedException e)
            {
                _logger.LogException(e);
                response = null;
            }

            return response;
        }

        public async Task<HttpResponseMessage> Fallback(string path = "/fail")
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

            return await policy.ExecuteAsync(() => _httpClient.GetAsync(path));
        }

        public async Task<HttpResponseMessage> PolicyWrap(string path = "/timeout/3")
        {
            var timeoutPolicy = Policy.TimeoutAsync<HttpResponseMessage>(3);
            var retryPolicy = HttpPolicyExtensions
                .HandleTransientHttpError()
                .Or<TimeoutRejectedException>() // thrown by Polly's TimeoutPolicy if the inner execution times out
                .RetryAsync(3);
            var wrappedPolicy = Policy.WrapAsync(retryPolicy, timeoutPolicy);

            HttpResponseMessage response;

            try
            {
                response = await wrappedPolicy.ExecuteAsync(async ct =>
                    await _httpClient.GetAsync(path, ct),
                    CancellationToken.None);
            }
            catch (TimeoutRejectedException e) 
            {
                _logger.LogException(e);
                response = null;
            }

            return response;
        }

        public async Task<HttpResponseMessage> CircuitBreaker(string path = "/fail")
        {
            var halfOpenCount = 0;
            var breakSeconds = 5;
            var circuitBreakerPolicy = HttpPolicyExtensions
                .HandleTransientHttpError()
                .CircuitBreakerAsync(
                    handledEventsAllowedBeforeBreaking: 3,
                    durationOfBreak: TimeSpan.FromSeconds(breakSeconds),
                    onBreak: (exception, timespan) => _logger.Failure($"The circuit is now open and is not allowing calls for the next {breakSeconds} seconds.", _noEOL),
                    onReset: () => _logger.Success("The circuit is now closed and all requests will be allowed."),
                    onHalfOpen: () =>
                    {
                        _logger.LineFeed().Warning("The circuit is now half-open and will allow one request.").ReadKey(true);
                        if (++halfOpenCount > 1) path = happyPath;
                    }
                );

            HttpResponseMessage response;

            while (true)
            {
                try
                {
                    response = await circuitBreakerPolicy.ExecuteAsync(() => _httpClient.GetAsync(path));
                    if (response.IsSuccessStatusCode) break;
                }
                catch (BrokenCircuitException)
                {
                    _logger.HandleException(++_exceptionCount);
                }
            }

            return response;
        }

        public async Task<HttpResponseMessage> AdvancedCircuitBreaker(string path = "/fail")
        {
            var policy = HttpPolicyExtensions
                .HandleTransientHttpError()
                .AdvancedCircuitBreakerAsync(
                    failureThreshold: 0.5, // Break on >=50% actions result in handled exceptions...
                    samplingDuration: TimeSpan.FromSeconds(10), // ... over any 10 second period
                    minimumThroughput: 8, // ... provided at least 8 actions in the 10 second period.
                    durationOfBreak: TimeSpan.FromSeconds(10), // Break for 10 seconds.
                    onBreak: (exception, timespan) => _logger.Failure("The circuit is now open and is not allowing calls for the next 10 seconds.", _noEOL),
                    onReset: () => _logger.Success("The circuit is now closed and all requests will be allowed."),
                    onHalfOpen: () => _logger.LineFeed().Warning("The circuit is now half-open and will allow one request.")
                );

            HttpResponseMessage response = null;

            while (true)
            {
                try
                {
                    response = await policy.ExecuteAsync(() => _httpClient.GetAsync(path));
                    if (response.IsSuccessStatusCode) break;
                }
                catch (BrokenCircuitException)
                {
                    path = happyPath;
                    _logger.HandleException(++_exceptionCount);
                }
            }

            return response;
        }

        public async Task<HttpResponseMessage> BulkheadIsolation(string path = "/")
        {
            HttpResponseMessage response;

            try
            {
                _logger.LogBulkheadSlots(_bulkheadPolicy);
                response = await _bulkheadPolicy.ExecuteAsync(() => _httpClient.GetAsync(path));
            }
            catch (BulkheadRejectedException e)
            {
                _logger.LogException(e);
                response = null;
            }

            return response;
        }

        static async Task ConfigureInStartup(string[] args)
        {
            var path = ComposePath(args);
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

            var serviceProvider = services.BuildServiceProvider();
            var app = serviceProvider.GetRequiredService<App>();

            await app.Run(path);
        }
    }
}
