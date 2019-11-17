using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using MuirDev.ConsoleTools;

namespace PollyDemo.App
{
    public enum ActionType
    {
        Send,
        Receive,
    }

    public interface IDemo
    {
        Task Run();
    }

    public static class DemoLogger
    {
        private static readonly FluentConsole _console = new FluentConsole();
        private static readonly LogOptions _noEOL = new LogOptions
        {
            IsEndOfLine = false,
        };

        public static void LogRequest(ActionType actionType, string endpoint) => LogRequest(actionType, HttpMethod.Get, endpoint);

        public static void LogRequest(ActionType actionType, HttpMethod method, string endpoint)
        {
            _console
                .LineFeed()
                .Info($"{actionType} request: ", _noEOL)
                .Warning($"{method.ToString().ToUpper()} http://localhost:5000/api/WeatherForecast{endpoint}", _noEOL)
                .LineFeed();
        }

        public static void LogResponse(ActionType actionType, HttpStatusCode statusCode, string content)
        {
            _console.Info($"{actionType} response: ", _noEOL);
            var isSuccessStatusCode = (int)statusCode >= 200 && (int)statusCode < 300;
            var logOptions = new LogOptions
            {
                ForegroundColor = isSuccessStatusCode
                    ? ConsoleColor.Green
                    : ConsoleColor.Red,
                IsEndOfLine = false,
            };
            _console.Info($"{(int)statusCode} {statusCode}", logOptions);
            if (!string.IsNullOrWhiteSpace(content)) _console.Info($" : {content}", logOptions);
            _console.LineFeed();
        }

        public static void LogException(Exception exception)
        {
            ConsoleTools.Failure($"{exception.GetType()}: {exception.Message}");
        }
    }
}
