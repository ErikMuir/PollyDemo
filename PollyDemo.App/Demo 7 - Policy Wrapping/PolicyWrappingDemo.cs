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

            Logger.LogRequest(ActionType.Sending, HttpMethod.Get, Constants.SlowRequest);

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
