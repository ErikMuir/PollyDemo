using System;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Polly;
using Polly.Extensions.Http;
using Polly.Timeout;

namespace PollyDemo.App
{
    public class Demos
    {
        private readonly HttpClient _httpClient;

        public Demos(HttpClient client)
        {
            _httpClient = client;
        }

        public async void HappyPath()
        {
            const string endpoint = "/";

            var response = await _httpClient.GetAsync(endpoint);
            var content = JsonConvert.DeserializeObject<string>(await response.Content?.ReadAsStringAsync());
        }

        public async void RetryWithoutPolly()
        {
            const string endpoint = "/fail/2";

            HttpResponseMessage response = null;
            for (int retryLimit = 0; retryLimit <= 3; retryLimit++)
            {
                response = await _httpClient.GetAsync(endpoint);
                if (response.IsSuccessStatusCode)
                {
                    break;
                }
            }

            var content = JsonConvert.DeserializeObject<string>(await response?.Content?.ReadAsStringAsync());
        }

        public async void HandleResultRetry()
        {
            const string endpoint = "/fail/2";

            var policy = Policy
                .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                .RetryAsync();

            var response = await policy.ExecuteAsync(() => _httpClient.GetAsync(endpoint));
            var content = JsonConvert.DeserializeObject<string>(await response.Content?.ReadAsStringAsync());
        }

        public async void HandleTransientHttpErrorRetry()
        {
            const string endpoint = "/fail/2";

            // Handles HttpRequestException, Http status codes >= 500 (server errors) and status code 408 (request timeout)
            var policy = HttpPolicyExtensions
                .HandleTransientHttpError()
                .RetryAsync();

            var response = await policy.ExecuteAsync(() => _httpClient.GetAsync(endpoint));
            var content = JsonConvert.DeserializeObject<string>(await response.Content?.ReadAsStringAsync());
        }

        public async void RetryForever()
        {
            const string endpoint = "/fail";

            var policy = HttpPolicyExtensions
                .HandleTransientHttpError()
                .RetryForeverAsync();

            var response = await policy.ExecuteAsync(() => _httpClient.GetAsync(endpoint));
            var content = JsonConvert.DeserializeObject<string>(await response.Content?.ReadAsStringAsync());
        }

        public async void RetryNTimes()
        {
            const string endpoint = "/fail/4";

            var policy = HttpPolicyExtensions
                .HandleTransientHttpError()
                .RetryAsync(3);

            var response = await policy.ExecuteAsync(() => _httpClient.GetAsync(endpoint));
            var content = JsonConvert.DeserializeObject<string>(await response.Content?.ReadAsStringAsync());
        }

        public async void WaitAndRetryLinear()
        {
            const string endpoint = "/fail/4";

            var policy = HttpPolicyExtensions
                .HandleTransientHttpError()
                .WaitAndRetryAsync(3, x => TimeSpan.FromMilliseconds(500));

            var response = await policy.ExecuteAsync(() => _httpClient.GetAsync(endpoint));
            var content = JsonConvert.DeserializeObject<string>(await response.Content?.ReadAsStringAsync());
        }

        public async void WaitAndRetryLogarithmic()
        {
            const string endpoint = "/fail/4";

            var policy = HttpPolicyExtensions
                .HandleTransientHttpError()
                .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt) / 2));

            var response = await policy.ExecuteAsync(() => _httpClient.GetAsync(endpoint));
            var content = JsonConvert.DeserializeObject<string>(await response.Content?.ReadAsStringAsync());
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
        }
    }
}
