using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Polly;
using Polly.Extensions.Http;
using MuirDev.ConsoleTools;

namespace PollyDemo.App
{
    public class App
    {
        private readonly HttpClient _httpClient;
        private static readonly FluentConsole _console = new FluentConsole();
        private static readonly LogOptions _noEOL = new LogOptions { IsEndOfLine = false };

        public App(HttpClient client)
        {
            _httpClient = client;
            _httpClient.GetAsync("/setup").Wait();
            Console.Clear();
        }

        private static void LogRequest(string endpoint)
        {
            _console
                .LineFeed()
                .Info("Sending request: ", _noEOL)
                .Warning($"GET http://localhost:5000/api/WeatherForecast{endpoint}");
        }

        private static async Task LogResponse(HttpResponseMessage response)
        {
            var isOk = response.IsSuccessStatusCode;
            var options = new LogOptions { ForegroundColor = isOk ? ConsoleColor.DarkGreen : ConsoleColor.DarkRed };
            var content = JsonConvert.DeserializeObject(await response.Content.ReadAsStringAsync());
            _console
                .Info("Received response: ", _noEOL)
                .Info($"{(int)response.StatusCode} {response.StatusCode}", options)
                .LineFeed()
                .Info($"Tomorrow's forecast: {content}", new LogOptions { ForegroundColor = ConsoleColor.DarkCyan });
        }

        private static void LogException(Exception exception)
        {
            _console.Failure($"{exception.GetType()}: {exception.Message}");
        }

        public async Task Run(string endpoint)
        {
            LogRequest(endpoint);

            var response = await _httpClient.GetAsync(endpoint);

            await LogResponse(response);
        }
    }
}
