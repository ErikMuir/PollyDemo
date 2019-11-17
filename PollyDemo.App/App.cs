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

        public App(HttpClient client)
        {
            _httpClient = client;
            _httpClient.GetAsync("/setup").Wait();
        }

        public async Task Run()
        {
            const string endpoint = "/";

            var response = await _httpClient.GetAsync(endpoint);

            await LogResponse(response);
        }

        private static async Task LogResponse(HttpResponseMessage response)
        {
            var content = JsonConvert.DeserializeObject(await response.Content.ReadAsStringAsync()) ?? "Unknown";
            Console.WriteLine($"Tomorrow's forecast: {content}");
        }

        private static void LogException(Exception exception)
        {
            ConsoleTools.Failure($"{exception.GetType()}: {exception.Message}");
        }
    }
}
