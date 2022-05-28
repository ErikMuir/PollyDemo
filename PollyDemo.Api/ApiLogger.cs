namespace PollyDemo.Api;

public class ApiLogger : FluentConsole
{
    private static readonly LogOptions _noEOL = new LogOptions(false);
    private static readonly LogOptions _endpoint = new LogOptions(ConsoleColor.DarkYellow, false);
    private static readonly LogOptions _success = new LogOptions(ConsoleColor.DarkGreen, false);
    private static readonly LogOptions _failure = new LogOptions(ConsoleColor.DarkRed, false);

    public void LogRequest(HttpRequest request)
    {
        this.LineFeed()
            .Info("Received request: ", _noEOL)
            .Info("GET http://localhost:5000/api/WeatherForecast", _endpoint)
            .LineFeed();
    }

    public void LogResponse(HttpStatusCode statusCode, string content = null)
    {
        var options = (int)statusCode >= 200 && (int)statusCode < 300 ? _success : _failure;
        this.Info("Sending response: ", _noEOL)
            .Info($"{(int)statusCode} {statusCode}{(content == null ? "" : $" - {content}")}", options)
            .LineFeed();
    }
}
