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

    public static class Utils
    {
        public static void WriteRequest(ActionType actionType, HttpMethod method, string endpoint, bool isEndOfLine = true)
        {
            Console.WriteLine();
            Console.Write($"{actionType} request: ");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write($"{method.ToString().ToUpper()} {Constants.BaseAddress}{endpoint}");
            if (actionType == ActionType.Sending) Console.Write(" ...");
            Console.ResetColor();
            if (isEndOfLine) Console.WriteLine();
        }

        public static void WriteResponse(ActionType actionType, HttpStatusCode statusCode, object content, bool isEndOfLine = true)
        {
            Console.Write($"{actionType} response: ");
            var isSuccessStatusCode = (int)statusCode >= 200 && (int)statusCode < 300;
            Console.ForegroundColor = isSuccessStatusCode ? ConsoleColor.Green : ConsoleColor.Red;
            Console.Write($"{(int)statusCode} {statusCode}");
            if (content != null) Console.Write($": {content}");
            Console.ResetColor();
            if (isEndOfLine) Console.WriteLine();
        }

        public static void WriteException(Exception exception)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"{exception.GetType()}: {exception.Message}");
            Console.ResetColor();
        }
    }
}
