using MuirDev.ConsoleTools.Logger;
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
        public const string Demo1 = "Without Polly";
        public const string Demo2 = "Fallback Policy";
        public const string Demo3 = "Retry Policy";
        public const string Demo4 = "Wait and Retry Policy";
        public const string Demo5 = "Policy Delegates";
        public const string Demo6 = "Timeout Policy";
        public const string Demo7 = "Policy Wrapping";
        public const string Demo8 = "Circuit Breaker Fails";
        public const string Demo9 = "Circuit Breaker Recovers";
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

    public static class LoggerExtensions
    {
        private static readonly LogOptions _noEOL = new LogOptions
        {
            IsEndOfLine = false,
        };
        
        public static void LogRequest(this Logger logger, ActionType actionType, HttpMethod method, string endpoint)
        {
            logger.LineFeed();
            logger.Info($"{actionType} request: ", _noEOL);
            logger.Warning($"{method.ToString().ToUpper()} {Constants.BaseAddress}{endpoint}", _noEOL);
            if (actionType == ActionType.Sending) logger.Warning(" ...", _noEOL);
            logger.LineFeed();
        }

        public static void LogResponse(this Logger logger, ActionType actionType, HttpStatusCode statusCode, object content)
        {
            logger.Info($"{actionType} response: ", _noEOL);
            var isSuccessStatusCode = (int)statusCode >= 200 && (int)statusCode < 300;
            var logOptions = new LogOptions
            {
                ForegroundColorOverride = isSuccessStatusCode 
                    ? ConsoleColor.Green 
                    : ConsoleColor.Red,
                IsEndOfLine = false,
            };
            logger.Custom($"{(int)statusCode} {statusCode}", logOptions);
            if (content != null) logger.Custom($": {content}", logOptions);
            logger.LineFeed();
        }

        public static void LogException(this Logger logger, Exception exception)
        {
            logger.Error($"{exception.GetType()}: {exception.Message}");
        }
    }
}
