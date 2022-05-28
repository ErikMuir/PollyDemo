global using System;
global using System.Net;
global using System.Net.Http;
global using System.Net.Http.Headers;
global using System.Text;
global using System.Text.Json;
global using System.Threading;
global using System.Threading.Tasks;
global using Microsoft.Extensions.DependencyInjection;
global using MuirDev.ConsoleTools;
global using Polly;
global using Polly.Bulkhead;
global using Polly.CircuitBreaker;
global using Polly.Extensions.Http;
global using Polly.Timeout;
global using PollyDemo.App;

public static class Globals
{
    private const string FallbackValue = "Same as today";
    private static readonly string _fallbackJson = JsonSerializer.Serialize(FallbackValue);
    private static readonly StringContent _fallbackContent = new StringContent(_fallbackJson);

    public const string HappyPath = "/";
    public static LogOptions NoEOL => new LogOptions(false);
    public static readonly HttpResponseMessage FallbackResponse = new HttpResponseMessage(HttpStatusCode.OK) { Content = _fallbackContent };
    public static readonly AsyncBulkheadPolicy BulkheadPolicy = Policy.BulkheadAsync(4, 2);

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
