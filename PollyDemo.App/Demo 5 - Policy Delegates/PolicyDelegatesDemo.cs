using Newtonsoft.Json;
using Polly;
using Polly.Retry;
using PollyDemo.Common;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace PollyDemo.App.Demos
{
    public class PolicyDelegatesDemo : IDemo
    {
        private HttpClient _httpClient;
        private readonly RetryPolicy<HttpResponseMessage> _httpRetryPolicy;

        public PolicyDelegatesDemo()
        {
            _httpRetryPolicy =
                Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                    .RetryAsync(3, onRetry: (httpResponseMessage, i) =>
                    {
                        if (httpResponseMessage.Result.StatusCode == HttpStatusCode.Unauthorized)
                        {
                            PerformReauthorization();
                        }
                    });
        }

        private void PerformReauthorization()
        {
            Console.WriteLine("Reauthenticating ...");
            _httpClient = GetHttpClient("GoodAuthCode");
        }

        public async Task Run()
        {
            Console.WriteLine("Demo 5 - Policy Delegates");

            _httpClient = GetHttpClient("BadAuthCode");

            Utils.WriteRequest(ActionType.Sending, HttpMethod.Get, Constants.AuthRequest);

            var response = await _httpRetryPolicy.ExecuteAsync(() => _httpClient.GetAsync(Constants.AuthRequest));
            var content = null as object;

            if (response.IsSuccessStatusCode)
                content = JsonConvert.DeserializeObject<int>(await response.Content.ReadAsStringAsync());
            else if (response.Content != null)
                content = await response.Content.ReadAsStringAsync();

            Utils.WriteResponse(ActionType.Received, response.StatusCode, content);
        }

        private HttpClient GetHttpClient(string authCookieValue)
        {
            var cookieContainer = new CookieContainer();
            var handler = new HttpClientHandler() { CookieContainer = cookieContainer };
            cookieContainer.Add(new Uri("http://localhost"), new Cookie("Auth", authCookieValue));

            var httpClient = new HttpClient(handler);
            httpClient.BaseAddress = new Uri(Constants.BaseAddress);
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            return httpClient;
        }
    }
}
