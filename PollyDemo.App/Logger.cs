using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using Polly.Bulkhead;
using MuirDev.ConsoleTools;

namespace PollyDemo.App
{
    public class Logger : FluentConsole
    {
        private static readonly LogOptions _noEOL = new LogOptions(false);
        private static readonly LogOptions _endpoint = new LogOptions(ConsoleColor.DarkYellow);
        private static readonly LogOptions _success = new LogOptions(ConsoleColor.DarkGreen);
        private static readonly LogOptions _failure = new LogOptions(ConsoleColor.DarkRed);
        private static readonly LogOptions _forecast = new LogOptions(ConsoleColor.DarkCyan);
        private static readonly LogOptions _bulkhead = new LogOptions(ConsoleColor.DarkGreen, false);
        private static readonly LogOptions _queue = new LogOptions(ConsoleColor.DarkGray, false);


        public void LogRequest()
        {
            this.LineFeed()
                .Info("Sending request: ", _noEOL)
                .Info("GET http://localhost:5000/api/WeatherForecast", _endpoint);
        }

        public void LogResponse(HttpResponseMessage response)
        {
            if (response == null) return;
            var options = response.IsSuccessStatusCode ? _success : _failure;
            var content = response.Content.ReadAsStringAsync().Result ?? "null";
            try { content = JsonSerializer.Deserialize<string>(content); } catch (Exception) { }

            this.Info("Received response: ", _noEOL)
                .Info($"{(int)response.StatusCode} {response.StatusCode}", options)
                .LineFeed();

            if (response.IsSuccessStatusCode)
            {
                this.Info($"Tomorrow's forecast: {content}", _forecast)
                    .LineFeed();
            }
        }

        public void LogException(Exception exception)
        {
            this.LineFeed()
                .Failure($"{exception.GetType()}: {exception.Message}");
        }

        public void HandleException(int exceptionCount)
        {
            if (exceptionCount % 16000 == 0) this.Failure(".", _noEOL);
        }

        public void LogBulkheadSlots(AsyncBulkheadPolicy policy)
        {
            string getSlots(int count)
            {
                var slots = new StringBuilder();
                for (var i = 0; i < count; i++)
                    slots.Append('\u2610').Append(' ');
                return slots.ToString();
            }

            var bulkheadSlots = getSlots(policy.BulkheadAvailableCount);
            var queueSlots = getSlots(policy.QueueAvailableCount);
            var originalEncoding = this.OutputEncoding;

            this.SetEncoding(Encoding.Unicode)
                .Info("Available slots: ", _noEOL)
                .Info(bulkheadSlots, _bulkhead)
                .Info(queueSlots, _queue)
                .LineFeed()
                .SetEncoding(originalEncoding);
        }
    }
}
