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

namespace PollyDemo.App
{
    public class App
    {
        private readonly IAppLogger _logger;
        private readonly HttpClient _httpClient;

        public App(IAppLogger logger, HttpClient client)
        {
            _logger = logger;
            _httpClient = client;
            _httpClient.GetAsync("/setup").Wait();
            _logger.Clear();
            _exceptionCount = 0;
        }

        #region
        private const string happyPathEndpoint = "/";
        private static int _exceptionCount;
        private static readonly AsyncBulkheadPolicy _bulkheadPolicy = Policy.BulkheadAsync(4, 2);
        #endregion

        public async Task Run(string endpoint)
        {
            _logger.LogRequest(endpoint);

            var response = await GetResponse(endpoint);

            _logger.LogResponse(response);
        }

        private async Task<HttpResponseMessage> GetResponse(string endpoint)
        {
            return await _httpClient.GetAsync(endpoint);
        }
    }
}
