using System;
using System.Net;
using Microsoft.AspNetCore.Http;
using MuirDev.ConsoleTools;

namespace PollyDemo.Api
{
    public interface IApiLogger
    {
        public void Clear();
        public void LogRequest(HttpRequest request);
        public void LogResponse(HttpStatusCode statusCode, string content = null);
    }

    public class ApiLogger : IApiLogger
    {
        private static readonly FluentConsole _console = new FluentConsole();
        private static readonly LogOptions _noEOL = new LogOptions(false);
        private static readonly LogOptions _endpoint = new LogOptions(ConsoleColor.DarkYellow, false);
        private static readonly LogOptions _success = new LogOptions(ConsoleColor.DarkGreen, false);
        private static readonly LogOptions _failure = new LogOptions(ConsoleColor.DarkRed, false);

        public ApiLogger() { }

        public void Clear()
        {
            _console.Clear();
        }

        public void LogRequest(HttpRequest request)
        {
            _console
                .LineFeed()
                .Info("Received request: ", _noEOL)
                .Info($"GET http://localhost:5000/api/WeatherForecast{request.Path}", _endpoint)
                .LineFeed();
        }

        public void LogResponse(HttpStatusCode statusCode, string content = null)
        {
            var options = (int)statusCode >= 200 && (int)statusCode < 300 ? _success : _failure;
            _console
                .Info("Sending response: ", _noEOL)
                .Info($"{(int)statusCode} {statusCode}{(content == null ? "" : $" - {content}")}", options)
                .LineFeed();
        }
    }
}
