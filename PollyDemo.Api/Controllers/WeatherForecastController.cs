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
            LogRequest();
            await Task.Delay(_simulateDataProcessing);
            return await Ok();
        }

        [HttpGet("/fail/{*count}")]
        public async Task<IActionResult> Fail(int count)
        {
            LogRequest();
            await Task.Delay(_simulateDataProcessing);
            return count > 0 && ++_failCount > count
                ? await Ok()
                : await InternalServerError();
        }

        [HttpGet("/bad-request")]
        public async Task<IActionResult> Bad()
        {
            LogRequest();
            await Task.Delay(_simulateDataProcessing);
            return await BadRequest();
        }

        [HttpGet("/auth")]
        public async Task<IActionResult> Auth()
        {
            LogRequest();
            await Task.Delay(_simulateDataProcessing);
            return Request.Headers["Authorization"] == "Bearer fresh-token"
                ? await Ok()
                : await Unauthorized();
        }

        [HttpGet("/timeout/{*count}")]
        public async Task<IActionResult> Timeout(int count)
        {
            LogRequest();
            await Task.Delay(_simulateDataProcessing);
            if (count > 0 && ++_failCount > count) return await Ok();
            await Task.Delay(_simulateHangingService);
            return await RequestTimeout();
        }

        [HttpGet("/setup")]
        public IActionResult Setup()
        {
            Console.Clear();
            _failCount = 0;
            return base.Ok();
        }


        #region "Demo Orchestration"

        private static int _failCount = 0;
        private static int _simulateDataProcessing = 250;
        private static int _simulateHangingService = 5000;
        private static readonly FluentConsole _console = new FluentConsole();

        private string GetForecast() => _summaries[new Random().Next(_summaries.Length)];
        private async new Task<IActionResult> Ok() => await SendResponse(HttpStatusCode.OK, GetForecast());
        private async new Task<IActionResult> BadRequest() => await SendResponse(HttpStatusCode.BadRequest);
        private async Task<IActionResult> InternalServerError() => await SendResponse(HttpStatusCode.InternalServerError);
        private async Task<IActionResult> RequestTimeout() => await SendResponse(HttpStatusCode.RequestTimeout);
        private async new Task<IActionResult> Unauthorized() => await SendResponse(HttpStatusCode.Unauthorized);

        private async Task<IActionResult> SendResponse(HttpStatusCode statusCode, string content = null)
        {
            LogResponse(statusCode);
            await Task.Delay(250);
            return StatusCode((int)statusCode, content);
        }

        private static readonly LogOptions _noEOL = new LogOptions { IsEndOfLine = false };

        private void LogRequest()
        {
            _console
                .LineFeed()
                .Info("Received request: ", _noEOL)
                .Warning($"GET http://localhost:5000/api/WeatherForecast{Request.Path}", _noEOL)
                .LineFeed();
        }

        private static void LogResponse(HttpStatusCode statusCode)
        {
            _console.Info("Sending response: ", _noEOL);
            var isOk = (int)statusCode >= 200 && (int)statusCode < 300;
            var options = new LogOptions { ForegroundColor = isOk ? ConsoleColor.DarkGreen : ConsoleColor.DarkRed };
            _console.Info($"{(int)statusCode} {statusCode}", options);
        }

        #endregion
    }
}
