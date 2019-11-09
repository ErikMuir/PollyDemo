using Polly;
using Polly.Fallback;
using Polly.Timeout;
using PollyDemo.Common;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace PollyDemo.App.Demos
{
    public class FallbackPolicyDemo : IDemo
    {
        private HttpClient _httpClient;
        private readonly FallbackPolicy<HttpResponseMessage> _fallbackPolicy;
        private readonly int _fallbackResult = 0;

        public FallbackPolicyDemo()
        {
            _fallbackPolicy = Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                .Or<TimeoutRejectedException>()
                .FallbackAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new ObjectContent(_fallbackResult.GetType(), _fallbackResult, new JsonMediaTypeFormatter())
                });
        }

        public async Task Run()
        {
            Console.WriteLine("Demo 2 - Fallback Policy");

            _httpClient = GetHttpClient();

            Logger.LogRequest(ActionType.Sending, HttpMethod.Get, Constants.FailRequest);

            var response = await _fallbackPolicy.ExecuteAsync(() => _httpClient.GetAsync(Constants.FailRequest));
            var content = await response.Content?.ReadAsStringAsync();

            Logger.LogResponse(ActionType.Received, response.StatusCode, content);
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
