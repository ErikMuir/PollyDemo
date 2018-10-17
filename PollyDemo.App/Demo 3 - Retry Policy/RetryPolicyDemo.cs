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
    public class RetryPolicyDemo : IDemo
    {
        private HttpClient _httpClient;
        private readonly RetryPolicy<HttpResponseMessage> _httpRetryPolicy;

        public RetryPolicyDemo()
        {
            _httpRetryPolicy =
                Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                    .RetryAsync(3);
        }

        public async Task Run()
        {
            Console.WriteLine("Demo 3 - Retry Policy");

            _httpClient = GetHttpClient();

            Utils.WriteRequest(ActionType.Sending, HttpMethod.Get, Constants.IrregularRequest);

            var response = await _httpRetryPolicy.ExecuteAsync(() => _httpClient.GetAsync(Constants.IrregularRequest));
            var content = null as object;

            if (response.IsSuccessStatusCode)
                content = JsonConvert.DeserializeObject<int>(await response.Content.ReadAsStringAsync());
            else if (response.Content != null)
                content = await response.Content.ReadAsStringAsync();

            Utils.WriteResponse(ActionType.Received, response.StatusCode, content);
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
