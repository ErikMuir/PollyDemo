using MuirDev.ConsoleTools.Logger;
using Newtonsoft.Json;
using Polly;
using Polly.Fallback;
using Polly.Retry;
using Polly.Timeout;
using Polly.Wrap;
using PollyDemo.Common;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace PollyDemo.App.Demos
{
    public class PolicyWrappingDemo : IDemo
    {
        private HttpClient _httpClient;
        private static readonly Logger _logger = new Logger();
        private readonly PolicyWrap<HttpResponseMessage> _policy;
        private readonly TimeoutPolicy _timeoutPolicy;
        private readonly RetryPolicy<HttpResponseMessage> _retryPolicy;
        private readonly FallbackPolicy<HttpResponseMessage> _fallbackPolicy;
        private readonly int _fallbackResult = 0;

        public PolicyWrappingDemo()
        {
            _timeoutPolicy = Policy.TimeoutAsync(2);

            _retryPolicy =
                Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                    .Or<TimeoutRejectedException>()
                    .RetryAsync(3);

            _fallbackPolicy = Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                .Or<TimeoutRejectedException>()
                .FallbackAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new ObjectContent(_fallbackResult.GetType(), _fallbackResult, new JsonMediaTypeFormatter())
                });

            _policy = Policy.WrapAsync(_fallbackPolicy, _retryPolicy).WrapAsync(_timeoutPolicy);
        }

        public async Task Run()
        {
            Console.WriteLine("Demo 7 - Policy Wrapping");

            _httpClient = GetHttpClient();

            _logger.LogRequest(ActionType.Sending, HttpMethod.Get, Constants.SlowRequest);

            //var response =
            //    await
            //    _fallbackPolicy.ExecuteAsync(() =>
            //        _retryPolicy.ExecuteAsync(() =>
            //            _timeoutPolicy.ExecuteAsync(async token =>
            //                await _httpClient.GetAsync(Constants.SlowRequest, token),
            //                CancellationToken.None)));

            var response = await _policy.ExecuteAsync(async token =>
                await _httpClient.GetAsync(Constants.SlowRequest, token),
                CancellationToken.None);
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
