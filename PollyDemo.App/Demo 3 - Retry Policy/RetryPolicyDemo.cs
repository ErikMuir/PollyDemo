using Newtonsoft.Json;
using Polly;
using PollyDemo.Common;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace PollyDemo.App.Demos
{
    public class RetryPolicyDemo : IDemo
    {
        private HttpClient _httpClient;

        public RetryPolicyDemo(HttpClient client)
        {
            _httpClient = client;
        }

        public async Task Run()
        {
            Console.WriteLine("Demo 3 - Retry Policy");

            Logger.LogRequest(ActionType.Sending, HttpMethod.Get, Constants.IrregularEndpoint);

            var httpRetryPolicy =
                Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                    .RetryAsync(3);

            var response = await httpRetryPolicy.ExecuteAsync(() => _httpClient.GetAsync(Constants.IrregularEndpoint));
            var content = JsonConvert.DeserializeObject<string>(await response.Content?.ReadAsStringAsync());

            Logger.LogResponse(ActionType.Received, response.StatusCode, content);
        }
    }
}
