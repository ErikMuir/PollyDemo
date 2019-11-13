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
        private static int _irregularRequestCount = 0;
        private static int _simulateDataProcessing = 250;
        private static int _simulateHangingService = 10000;

        [HttpGet("/")]
        public async Task<IActionResult> Get()
        {
            DemoLogger.LogRequest(ActionType.Receive, "/");
            await Task.Delay(_simulateDataProcessing);
            return OkResponse();
        }

        [HttpGet("/fail")]
        public async Task<IActionResult> Fail()
        {
            DemoLogger.LogRequest(ActionType.Receive, "/fail");
            await Task.Delay(_simulateDataProcessing);
            return ErrorResponse();
        }

        [HttpGet("/irregular")]
        public async Task<IActionResult> Irregular()
        {
            DemoLogger.LogRequest(ActionType.Receive, "/irregular");
            await Task.Delay(_simulateDataProcessing);
            var isFourthRequest = ++_irregularRequestCount % 4 == 0;
            return isFourthRequest ? OkResponse() : ErrorResponse();
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


        #region -- Demo Orchestration --

        [HttpGet("/clear")]
        public IActionResult Clear()
        {
            Console.Clear();
            return Ok();
        }

        [HttpGet("/shutdown")]
        public IActionResult Shutdown()
        {
            Environment.Exit(0);
            return Ok();
        }

        #endregion

        #region -- Private Helper Methods --

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
