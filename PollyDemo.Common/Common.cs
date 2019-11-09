using MuirDev.ConsoleTools;
using System;
using System.ComponentModel;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace PollyDemo.Common
{
    public static class Constants
    {
        public const string BaseAddress = "http://localhost:5000/api/credits/";
        public const string FailRequest = "fail/1234";
        public const string IrregularRequest = "irregular/1234";
        public const string AuthRequest = "auth/1234";
        public const string SlowRequest = "slow/1234";
    }

    public enum ActionType
    {
        [Description("Sending")]
        Sending,

        [Description("Received")]
        Received,
    }

    public interface IDemo
    {
        Task Run();
    }

    public static class Logger
    {
        private static readonly LogOptions _noEOL = new LogOptions
        {
            IsEndOfLine = false,
        };

        public static void LogRequest(ActionType actionType, HttpMethod method, string endpoint)
        {
            ConsoleTools.LineFeed();
            ConsoleTools.Info($"{actionType} request: ", _noEOL);
            ConsoleTools.Warning($"{method.ToString().ToUpper()} {Constants.BaseAddress}{endpoint}", _noEOL);
            if (actionType == ActionType.Sending) ConsoleTools.Warning(" ...", _noEOL);
            ConsoleTools.LineFeed();
        }

        public static void LogResponse(ActionType actionType, HttpStatusCode statusCode, object content)
        {
            ConsoleTools.Info($"{actionType} response: ", _noEOL);
            var isSuccessStatusCode = (int)statusCode >= 200 && (int)statusCode < 300;
            var logOptions = new LogOptions
            {
                ForegroundColor = isSuccessStatusCode
                    ? ConsoleColor.Green
                    : ConsoleColor.Red,
                IsEndOfLine = false,
            };
            ConsoleTools.Info($"{(int)statusCode} {statusCode}", logOptions);
            if (content != null) ConsoleTools.Info($": {content}", logOptions);
            ConsoleTools.LineFeed();
        }

        public static void LogException(Exception exception)
        {
            ConsoleTools.Failure($"{exception.GetType()}: {exception.Message}");
        }
    }
}
