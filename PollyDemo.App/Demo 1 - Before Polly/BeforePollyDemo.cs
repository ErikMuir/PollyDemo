using PollyDemo.Common;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace PollyDemo.App.Demos
{
    public class BeforePollyDemo : IDemo
    {
        private HttpClient _httpClient;

        public async Task Run()
        {
            Console.WriteLine("Demo 1 - Without Polly");

            _httpClient = GetHttpClient();

            Logger.LogRequest(ActionType.Sending, HttpMethod.Get, Constants.FailRequest);

            var response = await _httpClient.GetAsync(Constants.FailRequest);
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
