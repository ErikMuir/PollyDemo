using System;
using System.Net.Http;
using System.Text.Json;
using MuirDev.ConsoleTools;

namespace PollyDemo.App
{
    public interface IAppLogger
    {
        public void Clear();
        public void LogRequest(string endpoint);
        public void LogResponse(HttpResponseMessage response);
        public void LogException(Exception exception);
        public void HandleException(int exceptionCount);
    }

    public class AppLogger : IAppLogger
    {
        private static readonly FluentConsole _console = new FluentConsole();
        private static readonly LogOptions _noEOL = new LogOptions(false);
        private static readonly LogOptions _endpoint = new LogOptions(ConsoleColor.DarkYellow);
        private static readonly LogOptions _success = new LogOptions(ConsoleColor.DarkGreen);
        private static readonly LogOptions _failure = new LogOptions(ConsoleColor.DarkRed);
        private static readonly LogOptions _forecast = new LogOptions(ConsoleColor.DarkCyan);

        public AppLogger() { }

        public void Clear()
        {
            _console.Clear();
        }

        public void LogRequest(string endpoint)
        {
            _console
                .LineFeed()
                .Info("Sending request: ", _noEOL)
                .Info($"GET http://localhost:5000/api/WeatherForecast{endpoint}", _endpoint);
        }

        public void LogResponse(HttpResponseMessage response)
        {
            var options = response.IsSuccessStatusCode ? _success : _failure;
            var content = response.Content.ReadAsStringAsync().Result ?? "null";
            try { content = JsonSerializer.Deserialize<string>(content); } catch (Exception) { }

            _console
                .Info("Received response: ", _noEOL)
                .Info($"{(int)response.StatusCode} {response.StatusCode}", options)
                .LineFeed();

            if (response.IsSuccessStatusCode)
            {
                _console
                    .Info($"Tomorrow's forecast: {content}", _forecast)
                    .LineFeed();
            }
        }

        public void LogException(Exception exception)
        {
            _console.Failure($"{exception.GetType()}: {exception.Message}");
        }

        public void HandleException(int exceptionCount)
        {
            if (exceptionCount % 16000 == 0) _console.Failure(".", _noEOL);
        }
    }
}
