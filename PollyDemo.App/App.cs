using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Polly;
using Polly.CircuitBreaker;
using Polly.Extensions.Http;
using Polly.Timeout;
using Polly.Bulkhead;
using MuirDev.ConsoleTools;
using System.Text.Json;

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
            _logger.LogRequest();

            var response = await GetResponse(path);

            _logger.LogResponse(response);
        }

        private async Task<HttpResponseMessage> GetResponse(string path)
        {
            return await _httpClient.GetAsync(endpoint);
        }
    }
}
