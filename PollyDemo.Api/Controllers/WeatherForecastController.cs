using System;
using System.Net;
using System.Net.Http;
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
        public string Get()
        {
            return GetForecast();
        }

        [HttpGet("/fail")]
        public async Task<IActionResult> Fail()
        {
            Logger.LogRequest(ActionType.Received, HttpMethod.Get, Constants.FailEndpoint);
            await Task.Delay(_simulateDataProcessing);
            return ErrorResponse();
        }

        [HttpGet("/irregular")]
        public async Task<IActionResult> Irregular()
        {
            Logger.LogRequest(ActionType.Received, HttpMethod.Get, Constants.IrregularEndpoint);
            await Task.Delay(_simulateDataProcessing);
            var isFourthRequest = ++_irregularRequestCount % 4 == 0;
            return isFourthRequest ? OkResponse() : ErrorResponse();
        }

        [HttpGet("/auth")]
        public async Task<IActionResult> Auth()
        {
            Logger.LogRequest(ActionType.Received, HttpMethod.Get, Constants.AuthEndpoint);
            await Task.Delay(_simulateDataProcessing);
            var isAuthenticated = Request.Headers["Authorization"] == "Bearer fresh-token";
            return isAuthenticated ? OkResponse() : UnauthorizedResponse();
        }

        [HttpGet("/slow")]
        public async Task<IActionResult> Slow()
        {
            Logger.LogRequest(ActionType.Received, HttpMethod.Get, Constants.SlowEndpoint);
            await Task.Delay(_simulateHangingService);
            return TimeoutResponse();
        }


        #region -- Demo Orchestration --

        [HttpGet("/clear")]
        public IActionResult Clear()
        {
            Console.Clear();
            Console.WriteLine("Now listening on: http://localhost:5000");
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
            Logger.LogResponse(ActionType.Sending, statusCode, content);
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
