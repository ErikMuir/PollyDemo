using Microsoft.AspNetCore.Mvc;
using MuirDev.ConsoleTools.Logger;
using PollyDemo.Common;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace PollyDemo.Api
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    public class CreditsController : Controller
    {
        private static readonly Logger _logger = new Logger();
        private static int _requestCount = 0;

        [HttpGet("fail/{userId}")]
        public async Task<IActionResult> Fail(int userId)
        {
            _logger.LogRequest(ActionType.Received, HttpMethod.Get, $"fail/{userId}");

            await Task.Delay(100); // simulate some data processing

            var statusCode = HttpStatusCode.InternalServerError;
            var content = "Something went wrong";

            _logger.LogResponse(ActionType.Sending, statusCode, content);

            return StatusCode((int)statusCode, content);
        }

        [HttpGet("irregular/{userId}")]
        public async Task<IActionResult> Irregular(int userId)
        {
            _logger.LogRequest(ActionType.Received, HttpMethod.Get, $"irregular/{userId}");

            await Task.Delay(100); // simulate some data processing

            _requestCount++;

            var isFourthRequest = _requestCount % 4 == 0;
            var statusCode = isFourthRequest ? HttpStatusCode.OK : HttpStatusCode.InternalServerError;
            var content = isFourthRequest ? 15 as object : "Something went wrong";

            _logger.LogResponse(ActionType.Sending, statusCode, content);

            return StatusCode((int)statusCode, content);
        }

        [HttpGet("auth/{userId}")]
        public async Task<IActionResult> Auth(int userId)
        {
            _logger.LogRequest(ActionType.Received, HttpMethod.Get, $"auth/{userId}");

            await Task.Delay(100); // simulate some data processing

            var isAuthenticated = Request.Cookies["Auth"] == "GoodAuthCode";
            var statusCode = isAuthenticated ? HttpStatusCode.OK : HttpStatusCode.Unauthorized;
            var content = isAuthenticated ? 15 as object : "You are not authorized";

            _logger.LogResponse(ActionType.Sending, statusCode, content);

            return StatusCode((int)statusCode, content);
        }

        [HttpGet("slow/{userId}")]
        public async Task<IActionResult> Slow(int userId)
        {
            _logger.LogRequest(ActionType.Received, HttpMethod.Get, $"slow/{userId}");

            await Task.Delay(10000); // simulate some heavy data processing by delaying for 10 seconds

            var statusCode = HttpStatusCode.InternalServerError;
            var content = "Something went wrong";

            _logger.LogResponse(ActionType.Sending, statusCode, content);

            return StatusCode((int)statusCode, content);
        }

        #region -- Demo Orchestration --

        [HttpGet("clear")]
        public IActionResult Clear()
        {
            Console.Clear();
            Console.WriteLine("Now listening on: http://localhost:5000");
            return Ok();
        }

        [HttpGet("shutdown")]
        public IActionResult Shutdown()
        {
            Environment.Exit(0);
            return Ok();
        }

        #endregion
    }
}
