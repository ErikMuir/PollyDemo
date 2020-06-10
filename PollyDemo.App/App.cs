using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Polly;
using Polly.Bulkhead;
using Polly.CircuitBreaker;
using Polly.Extensions.Http;
using Polly.Timeout;
using MuirDev.ConsoleTools;

namespace PollyDemo.App
{
    public class App
    {
        private static readonly AppLogger _logger = new AppLogger();
        private readonly HttpClient _httpClient;

        public App(HttpClient client)
        {
            _httpClient = client;
            _httpClient.GetAsync("/setup").Wait();
            _logger.Clear();
            _exceptionCount = 0;
        }

        #region
        private const string happyPath = "/";
        private static int _exceptionCount;
        private static readonly LogOptions _noEOL = new LogOptions(false);
        private static readonly AsyncBulkheadPolicy _bulkheadPolicy = Policy.BulkheadAsync(4, 2);
        #endregion

        public async Task Run(string path)
        {
            // await DrillBabyDrill();

            _logger.LogRequest();

            var response = await GetResponse(path);

            _logger.LogResponse(response);
        }

        private async Task<HttpResponseMessage> GetResponse(string path)
        {
            return await _httpClient.GetAsync(path);
        }

        private async Task DrillBabyDrill()
        {
            // utilize all 4 bulkhead slots and 2 queue slots
            // then one more call to see the bulkhead exception
            for (var i = 0; i < 7; i++)
            {
                await Task.Delay(50);
                _logger.LogRequest();
                GetResponse(happyPath).GetAwaiter();
            }

            // wait for a slot to free up
            await Task.Delay(500);
        }
    }
}
