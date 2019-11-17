using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MuirDev.ConsoleTools;

namespace PollyDemo.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] _summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };


        [HttpGet("/")]
        public async Task<IActionResult> Get()
        {
            LogRequest("/");
            await Task.Delay(_simulateDataProcessing);
            return await Ok();
        }

        [HttpGet("/fail/{*count}")]
        public async Task<IActionResult> Fail(int count)
        {
            LogRequest("/fail");
            await Task.Delay(_simulateDataProcessing);
            if (count == 0) return await InternalServerError();
            var isOk = ++_failCount > count;
            return isOk ? await Ok() : await InternalServerError();
        }

        [HttpGet("/bad-request")]
        public async Task<IActionResult> Bad()
        {
            LogRequest("/bad-request");
            await Task.Delay(_simulateDataProcessing);
            return await BadRequest();
        }

        [HttpGet("/auth")]
        public async Task<IActionResult> Auth()
        {
            LogRequest("/auth");
            await Task.Delay(_simulateDataProcessing);
            var isAuthenticated = Request.Headers["Authorization"] == "Bearer fresh-token";
            return isAuthenticated ? await Ok() : await Unauthorized();
        }

        [HttpGet("/timeout/{*count}")]
        public async Task<IActionResult> Timeout(int count)
        {
            LogRequest("/timeout");
            if (++_failCount > count) return await Ok();
            await Task.Delay(_simulateHangingService);
            return await RequestTimeout();
        }


        #region "Demo Orchestration"

        private static int _failCount = 0;
        private static int _simulateDataProcessing = 250;
        private static int _simulateHangingService = 5000;

        private async new Task<IActionResult> Ok() => await SendResponse(HttpStatusCode.OK, GetForecast());
        private async new Task<IActionResult> BadRequest() => await SendResponse(HttpStatusCode.BadRequest);
        private async Task<IActionResult> InternalServerError() => await SendResponse(HttpStatusCode.InternalServerError);
        private async Task<IActionResult> RequestTimeout() => await SendResponse(HttpStatusCode.RequestTimeout);
        private async new Task<IActionResult> Unauthorized() => await SendResponse(HttpStatusCode.Unauthorized);

        private async Task<IActionResult> SendResponse(HttpStatusCode statusCode, string content = null)
        {
            LogResponse(statusCode, content);
            await Task.Delay(250);
            return StatusCode((int)statusCode, content);
        }

        private string GetForecast()
        {
            var rng = new Random();
            return _summaries[rng.Next(_summaries.Length)];
        }

        [HttpGet("/clear")]
        public IActionResult Clear()
        {
            Console.Clear();
            _failCount = 0;
            return base.Ok();
        }

        [HttpGet("/shutdown")]
        public IActionResult Shutdown()
        {
            Environment.Exit(0);
            return base.Ok();
        }

        private static readonly LogOptions _noEOL = new LogOptions { IsEndOfLine = false };

        private static void LogRequest(string endpoint)
        {
            ConsoleTools.LineFeed();
            ConsoleTools.Info("Received request: ", _noEOL);
            ConsoleTools.Warning($"GET http://localhost:5000/api/WeatherForecast{endpoint}", _noEOL);
            ConsoleTools.LineFeed();
        }

        private static void LogResponse(HttpStatusCode statusCode, string content)
        {
            ConsoleTools.Info("Sending response: ", _noEOL);
            var isSuccessStatusCode = (int)statusCode >= 200 && (int)statusCode < 300;
            var logOptions = new LogOptions
            {
                ForegroundColor = isSuccessStatusCode
                    ? ConsoleColor.DarkGreen
                    : ConsoleColor.DarkRed,
                IsEndOfLine = false,
            };
            ConsoleTools.Info($"{(int)statusCode} {statusCode}", logOptions);
            if (!string.IsNullOrWhiteSpace(content)) ConsoleTools.Info($" : {content}", logOptions);
            ConsoleTools.LineFeed();
        }

        #endregion
    }
}
