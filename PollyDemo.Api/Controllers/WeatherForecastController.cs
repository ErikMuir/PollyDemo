using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PollyDemo.Common;

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
            DemoLogger.LogRequest(ActionType.Receive, "/");
            await Task.Delay(_simulateDataProcessing);
            return OkResponse();
        }

        [HttpGet("/fail/{count}")]
        public async Task<IActionResult> Fail(int count)
        {
            DemoLogger.LogRequest(ActionType.Receive, "/fail");
            await Task.Delay(_simulateDataProcessing);
            if (count == 0) return ErrorResponse();
            return ++_failCount > count ? OkResponse() : ErrorResponse();
        }

        [HttpGet("/auth")]
        public async Task<IActionResult> Auth()
        {
            DemoLogger.LogRequest(ActionType.Receive, "/auth");
            await Task.Delay(_simulateDataProcessing);
            var isAuthenticated = Request.Headers["Authorization"] == "Bearer fresh-token";
            return isAuthenticated ? OkResponse() : UnauthorizedResponse();
        }

        [HttpGet("/timeout")]
        public async Task<IActionResult> Timeout()
        {
            DemoLogger.LogRequest(ActionType.Receive, "/timeout");
            await Task.Delay(_simulateHangingService);
            return TimeoutResponse();
        }


        #region "Demo Orchestration"

        private static int _failCount = 0;
        private static int _simulateDataProcessing = 500;
        private static int _simulateHangingService = 10000;

        [HttpGet("/clear")]
        public IActionResult Clear()
        {
            Console.Clear();
            _failCount = 0;
            return Ok();
        }

        [HttpGet("/shutdown")]
        public IActionResult Shutdown()
        {
            Environment.Exit(0);
            return Ok();
        }

        private IActionResult SendResponse(HttpStatusCode statusCode, string content = null)
        {
            DemoLogger.LogResponse(ActionType.Send, statusCode, content);
            return StatusCode((int)statusCode, content);
        }

        private IActionResult OkResponse() => SendResponse(HttpStatusCode.OK, GetForecast());

        private IActionResult ErrorResponse() => SendResponse(HttpStatusCode.InternalServerError);

        private IActionResult TimeoutResponse() => SendResponse(HttpStatusCode.RequestTimeout);

        private IActionResult UnauthorizedResponse() => SendResponse(HttpStatusCode.Unauthorized);

        private string GetForecast()
        {
            var rng = new Random();
            return _summaries[rng.Next(_summaries.Length)];
        }

        #endregion
    }
}
