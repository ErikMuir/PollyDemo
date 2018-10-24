using MuirDev.ConsoleTools.Logger;
using Newtonsoft.Json;
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
        private static readonly Logger _logger = new Logger();
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

            _logger.LogRequest(ActionType.Sending, HttpMethod.Get, Constants.FailRequest);

            var response = await _fallbackPolicy.ExecuteAsync(() => _httpClient.GetAsync(Constants.FailRequest));
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
