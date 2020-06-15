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
    public partial class App
    {
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
    }
}
