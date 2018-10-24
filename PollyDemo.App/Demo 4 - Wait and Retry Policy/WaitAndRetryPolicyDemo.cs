using MuirDev.ConsoleTools.Logger;
using Newtonsoft.Json;
using Polly;
using Polly.Retry;
using PollyDemo.Common;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace PollyDemo.App.Demos
{
    public class WaitAndRetryPolicyDemo : IDemo
    {
        private HttpClient _httpClient;
        private static readonly Logger _logger = new Logger();
        private readonly RetryPolicy<HttpResponseMessage> _httpRetryPolicy;

        public WaitAndRetryPolicyDemo()
        {
            _httpRetryPolicy =
                Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                    .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt) / 2));
                    //.WaitAndRetryAsync(new[]
                    //{
                    //    TimeSpan.FromSeconds(1),
                    //    TimeSpan.FromSeconds(2),
                    //    TimeSpan.FromSeconds(3),
                    //});

        }

        public async Task Run()
        {
            Console.WriteLine("Demo 4 - Wait and Retry Policy");

            _httpClient = GetHttpClient();

            _logger.LogRequest(ActionType.Sending, HttpMethod.Get, Constants.IrregularRequest);

            var response = await _httpRetryPolicy.ExecuteAsync(() => _httpClient.GetAsync(Constants.IrregularRequest));
            var content = null as object;

            if (response.IsSuccessStatusCode)
                content = JsonConvert.DeserializeObject<int>(await response.Content.ReadAsStringAsync());
            else if (response.Content != null)
                content = await response.Content.ReadAsStringAsync();

            _logger.LogResponse(ActionType.Received, response.StatusCode, content);
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
