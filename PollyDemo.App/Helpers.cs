using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using MuirDev.ConsoleTools;
using Polly;
using Polly.Bulkhead;

namespace PollyDemo.App
{
    public static class Helpers
    {
        public static string ComposePath(string[] args)
        {
            var path = "/";

            if (args.Length > 0)
                path += args[0].ToLower();

            if (args.Length > 1 && (path == "/fail" || path == "/timeout"))
            {
                int.TryParse(args[1], out var count);
                path += $"/{count}";
            }

            return path;
        }
    }

    public static class Extensions
    {
        public static FluentConsole SetEncoding(this FluentConsole logger, Encoding encoding)
        {
            logger.OutputEncoding = encoding;
            return logger;
        }
    }

    public partial class App
    {
        private static readonly Logger _logger = new Logger();
        private readonly HttpClient _httpClient;
        private const string happyPath = "/";
        private static int _exceptionCount;
        private static readonly LogOptions _noEOL = new LogOptions(false);
        private static readonly string _fallbackValue = "Same as today";
        private static readonly string _fallbackJson = JsonSerializer.Serialize(_fallbackValue);
        private static readonly StringContent _fallbackContent = new StringContent(_fallbackJson);
        private static readonly HttpResponseMessage _fallbackResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = _fallbackContent
        };
        private static readonly AsyncBulkheadPolicy _bulkheadPolicy = Policy.BulkheadAsync(4, 2);

        public App(HttpClient client)
        {
            _httpClient = client;
            _httpClient.GetAsync("/setup").Wait();
            _logger.Clear();
            _exceptionCount = 0;
        }

        private async Task DrillBabyDrill()
        {
            // utilize all 4 bulkhead slots and 2 queue slots
            // then one more call to see the bulkhead exception
            for (var i = 0; i < 7; i++)
            {
                await Task.Delay(50);
                GetResponse(happyPath).GetAwaiter();
            }

            // then wait for a slot to free up
            await Task.Delay(500);
        }
    }

    public partial class Demos
    {
        private readonly HttpClient _httpClient;
        private readonly AsyncBulkheadPolicy _bulkheadPolicy = Policy.BulkheadAsync(4, 2);
        private const string happyPath = "/";
        private static readonly LogOptions _noEOL = new LogOptions(false);
        private static readonly Logger _logger = new Logger();
        private static int _exceptionCount = 0;
        private static readonly HttpResponseMessage _fallbackResponse = new HttpResponseMessage();
        private async Task<HttpResponseMessage> GetResponse(string path) => await Task.FromResult(new HttpResponseMessage());
        public Demos(HttpClient client)
        {
            _httpClient = client;
        }
    }
}
