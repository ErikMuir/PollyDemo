using System;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Polly;
using Polly.Extensions.Http;
using Polly.Timeout;

namespace PollyDemo.App.Demos
{
    public partial class Demos
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

        public async void WithoutPolly()
        {
            const string endpoint = "/fail";

            var response = await _httpClient.GetAsync(endpoint);
            var content = JsonConvert.DeserializeObject<string>(await response.Content?.ReadAsStringAsync());
        }

        public async void RetryOnce()
        {
            const string endpoint = "/fail";

            var policy = Policy
                .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                .Or<HttpRequestException>()
                .RetryAsync();

            var response = await policy.ExecuteAsync(() => _httpClient.GetAsync(endpoint));
            var content = JsonConvert.DeserializeObject<string>(await response.Content?.ReadAsStringAsync());
        }

        public async void RetryForever()
        {
            const string endpoint = "/fail";

            var policy = Policy
                .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                .Or<HttpRequestException>()
                .RetryForeverAsync();

            var response = await policy.ExecuteAsync(() => _httpClient.GetAsync(endpoint));
            var content = JsonConvert.DeserializeObject<string>(await response.Content?.ReadAsStringAsync());
        }

        public async void RetryNTimes()
        {
            const string endpoint = "/irregular";

            var policy = Policy
                .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                .Or<HttpRequestException>()
                .RetryAsync(3);

            var response = await policy.ExecuteAsync(() => _httpClient.GetAsync(endpoint));
            var content = JsonConvert.DeserializeObject<string>(await response.Content?.ReadAsStringAsync());
        }

        public async void WaitAndRetryLinear()
        {
            const string endpoint = "/irregular";

            var policy = Policy
                .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                .Or<HttpRequestException>()
                .WaitAndRetryAsync(3, x => TimeSpan.FromMilliseconds(500));

            var response = await policy.ExecuteAsync(() => _httpClient.GetAsync(endpoint));
            var content = JsonConvert.DeserializeObject<string>(await response.Content?.ReadAsStringAsync());
        }

        public async void WaitAndRetryLogarithmic()
        {
            const string endpoint = "/irregular";

            var policy = Policy
                .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                .Or<HttpRequestException>()
                .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt) / 2));

            var response = await policy.ExecuteAsync(() => _httpClient.GetAsync(endpoint));
            var content = JsonConvert.DeserializeObject<string>(await response.Content?.ReadAsStringAsync());
        }

        public async void HandleTransientHttpError()
        {
            const string endpoint = "/irregular";

            // Handles HttpRequestException, Http status codes >= 500 (server errors) and status code 408 (request timeout)
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
                .AddHttpClient<AppClient>(x =>
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
