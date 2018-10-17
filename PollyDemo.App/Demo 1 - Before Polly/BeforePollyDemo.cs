using Newtonsoft.Json;
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

            Utils.WriteRequest(ActionType.Sending, HttpMethod.Get, Constants.FailRequest);

            var response = await _httpClient.GetAsync(Constants.FailRequest);
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
