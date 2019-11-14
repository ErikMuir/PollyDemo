using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Polly;
using PollyDemo.Common;

namespace PollyDemo.App
{
    public class Retry : IDemo
    {
        private HttpClient _httpClient;

        public Retry(HttpClient client)
        {
            _httpClient = client;
        }

        public async Task Run()
        {
            Console.Clear();
            Console.WriteLine("Demo 3 - Retry Policy");
            Console.ReadKey(true);

            DemoLogger.LogRequest(ActionType.Send, "/fail/4");

            var httpRetryPolicy =
                Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                    .RetryAsync(3);

            var response = await httpRetryPolicy.ExecuteAsync(() => _httpClient.GetAsync("/fail/4"));
            var content = JsonConvert.DeserializeObject<string>(await response.Content?.ReadAsStringAsync());

            DemoLogger.LogResponse(ActionType.Receive, response.StatusCode, content);
        }
    }
}
