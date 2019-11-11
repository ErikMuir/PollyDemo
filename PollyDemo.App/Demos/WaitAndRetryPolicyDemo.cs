using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Polly;
using PollyDemo.Common;

namespace PollyDemo.App.Demos
{
    public class WaitAndRetryPolicyDemo : IDemo
    {
        private HttpClient _httpClient;

        public WaitAndRetryPolicyDemo(HttpClient client)
        {
            _httpClient = client;
        }

        public async Task Run()
        {
            Console.Clear();
            Console.WriteLine("Demo 4 - Wait and Retry Policy");
            Console.ReadKey(true);

            Logger.LogRequest(ActionType.Send, HttpMethod.Get, Constants.IrregularEndpoint);

            var httpRetryPolicy =
                Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                    .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt) / 2));
            //.WaitAndRetryAsync(new[]
            //{
            //    TimeSpan.FromSeconds(1),
            //    TimeSpan.FromSeconds(2),
            //    TimeSpan.FromSeconds(3),
            //});

            var response = await httpRetryPolicy.ExecuteAsync(() => _httpClient.GetAsync(Constants.IrregularEndpoint));
            var content = JsonConvert.DeserializeObject<string>(await response.Content?.ReadAsStringAsync());

            Logger.LogResponse(ActionType.Receive, response.StatusCode, content);
        }
    }
}
