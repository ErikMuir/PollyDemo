using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Polly;
using Polly.CircuitBreaker;
using Polly.Extensions.Http;
using Polly.Timeout;
using MuirDev.ConsoleTools;

namespace PollyDemo.App
{
    public class App
    {
        private readonly HttpClient _httpClient;

        public App(HttpClient client)
        {
            _httpClient = client;
            _httpClient.GetAsync("/setup").Wait();
            _console.Clear();
            _exceptionCount = 0;
        }

        #region [Demo Orchestration]

        private static readonly FluentConsole _console = new FluentConsole();
        private static int _exceptionCount;
        private static readonly LogOptions _noEOL = new LogOptions(false);
        private static readonly LogOptions _endpoint = new LogOptions(ConsoleColor.DarkYellow);
        private static readonly LogOptions _success = new LogOptions(ConsoleColor.DarkGreen);
        private static readonly LogOptions _failure = new LogOptions(ConsoleColor.DarkRed);
        private static readonly LogOptions _forecast = new LogOptions(ConsoleColor.DarkCyan);

        private static void LogRequest(string endpoint)
        {
            _console
                .LineFeed()
                .Info("Sending request: ", _noEOL)
                .Info($"GET http://localhost:5000/api/WeatherForecast{endpoint}", _endpoint);
        }

        private static void LogResponse(HttpResponseMessage response)
        {
            var options = response.IsSuccessStatusCode ? _success : _failure;
            var content = JsonConvert.DeserializeObject(response.Content.ReadAsStringAsync().Result) ?? "null";
            _console
                .Info("Received response: ", _noEOL)
                .Info($"{(int)response.StatusCode} {response.StatusCode}", options)
                .LineFeed()
                .Info($"Tomorrow's forecast: {content}", _forecast);
        }

        private static void LogException(Exception exception)
        {
            _console.Failure($"{exception.GetType()}: {exception.Message}");
        }

        private static void HandleException()
        {
            if (++_exceptionCount % 5000 == 0) _console.Failure(".", _noEOL);
        }

        #endregion
        
        public async Task Run(string endpoint)
        {
            LogRequest(endpoint);

            var response = await GetResponse(endpoint);

            LogResponse(response);
        }

        private async Task<HttpResponseMessage> GetResponse(string endpoint)
        {
            return await _httpClient.GetAsync(endpoint);
        }
    }
}
