namespace PollyDemo.App;

public class Demos
{
    private static readonly AppLogger _logger = new AppLogger();
    private static int _exceptionCount = 0;
    private readonly HttpClient _httpClient;

    public Demos(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<HttpResponseMessage?> RetryWithoutPolly(string path = "/fail/1")
    {
        HttpResponseMessage? response = null;

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

    public async Task<HttpResponseMessage?> Handling(string path = "/fail/1")
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
    }

    public async Task<HttpResponseMessage?> Retry(string path = "/fail/1")
    {
        // problems may last more than an instant
        // fail 3
        var policy = HttpPolicyExtensions
            .HandleTransientHttpError()
            .RetryAsync(3);

        return await policy.ExecuteAsync(() => _httpClient.GetAsync(path));

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

    public async Task<HttpResponseMessage?> HttpRequestException(string path = "/")
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

    public async Task<HttpResponseMessage?> Delegates(string path = "/auth")
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

    public async Task<HttpResponseMessage?> Timeout(string path = "/timeout")
    {
        var policy = Policy.TimeoutAsync(3);

        HttpResponseMessage? response;
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

    public async Task<HttpResponseMessage?> Fallback(string path = "/fail")
    {
        var policy = HttpPolicyExtensions
            .HandleTransientHttpError()
            .FallbackAsync(Globals.FallbackResponse);

        return await policy.ExecuteAsync(() => _httpClient.GetAsync(path));
    }

    public async Task<HttpResponseMessage?> PolicyWrap(string path = "/fail/4")
    {
        var timeoutPolicy = Policy.TimeoutAsync<HttpResponseMessage>(3);
        var retryPolicy = HttpPolicyExtensions
            .HandleTransientHttpError()
            .Or<TimeoutRejectedException>()
            .RetryAsync(3);
        var fallbackPolicy = HttpPolicyExtensions
            .HandleTransientHttpError()
            .FallbackAsync(Globals.FallbackResponse);

        var wrappedPolicy = Policy.WrapAsync(fallbackPolicy, retryPolicy, timeoutPolicy);

        return await wrappedPolicy.ExecuteAsync(() => _httpClient.GetAsync(path));
    }

    public async Task<HttpResponseMessage?> CircuitBreaker(string path = "/fail")
    {
        var halfOpenCount = 0;
        var breakSeconds = 5;
        var circuitBreakerPolicy = HttpPolicyExtensions
            .HandleTransientHttpError()
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: 3,
                durationOfBreak: TimeSpan.FromSeconds(breakSeconds),
                onBreak: (exception, timespan) => _logger.Failure($"The circuit is now open and is not allowing calls for the next {breakSeconds} seconds.", Globals.NoEOL),
                onReset: () => _logger.Success("The circuit is now closed and all requests will be allowed."),
                onHalfOpen: () =>
                {
                    _logger.LineFeed().Warning("The circuit is now half-open and will allow one request.").ReadKey(true);
                    if (++halfOpenCount > 1) path = Globals.HappyPath;
                }
            );

        HttpResponseMessage? response;

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

    public async Task<HttpResponseMessage?> AdvancedCircuitBreaker(string path = "/fail")
    {
        var policy = HttpPolicyExtensions
            .HandleTransientHttpError()
            .AdvancedCircuitBreakerAsync(
                durationOfBreak: TimeSpan.FromSeconds(10),  // the circuit breaks for 10 seconds
                failureThreshold: 0.1,                      // if there is a 10% failure rate
                samplingDuration: TimeSpan.FromSeconds(60), // in a 60 second window
                minimumThroughput: 100                      // with a minimum of 100 requests
            );

        return await policy.ExecuteAsync(() => _httpClient.GetAsync(path));
    }

    public async Task<HttpResponseMessage?> BulkheadIsolation(string path = "/")
    {
        HttpResponseMessage? response;

        try
        {
            _logger.LogBulkheadSlots(Globals.BulkheadPolicy);
            response = await Globals.BulkheadPolicy.ExecuteAsync(() => _httpClient.GetAsync(path));
        }
        catch (BulkheadRejectedException e)
        {
            _logger.LogException(e);
            response = null;
        }

        return response;
    }

    static void ConfigureInStartup(ServiceCollection services)
    {
        var retryPolicy = HttpPolicyExtensions
            .HandleTransientHttpError()
            .Or<TimeoutRejectedException>()
            .RetryAsync(3);

        var timeoutPolicy = Policy.TimeoutAsync<HttpResponseMessage>(10);

        var wrappedPolicy = Policy.WrapAsync(retryPolicy, timeoutPolicy);

        services
            .AddHttpClient<App>(x =>
            {
                x.BaseAddress = new Uri("http://localhost:5000/api/WeatherForecast");
                x.DefaultRequestHeaders.Accept.Clear();
                x.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            })
            .AddPolicyHandler(wrappedPolicy);
    }
}
