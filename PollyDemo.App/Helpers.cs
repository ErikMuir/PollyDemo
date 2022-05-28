namespace PollyDemo.App;

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
    private static int _exceptionCount = 0;
    private static readonly LogOptions _noEOL = new LogOptions(false);
    private static readonly string _fallbackValue = "Same as today";
    private static readonly string _fallbackJson = JsonSerializer.Serialize(_fallbackValue);
    private static readonly StringContent _fallbackContent = new StringContent(_fallbackJson);
    private static readonly HttpResponseMessage _fallbackResponse = new HttpResponseMessage(HttpStatusCode.OK)
    {
        Content = _fallbackContent
    };
    private static readonly AsyncBulkheadPolicy _bulkheadPolicy = Policy.BulkheadAsync(4, 2);
}

public partial class Demos
{
    private readonly HttpClient _httpClient;
    private readonly AsyncBulkheadPolicy _bulkheadPolicy = Policy.BulkheadAsync(4, 2);
    private const string happyPath = "/";
    private static readonly LogOptions _noEOL = new LogOptions(false);
    private static readonly AppLogger _logger = new AppLogger();
    private static int _exceptionCount = 0;
    private static readonly HttpResponseMessage _fallbackResponse = new HttpResponseMessage();
    private async Task<HttpResponseMessage> GetResponse(string path) => await Task.FromResult(new HttpResponseMessage());
    public Demos(HttpClient client)
    {
        _httpClient = client;
    }
}
