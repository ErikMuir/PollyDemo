using Microsoft.AspNetCore.Mvc;
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
        private static int _irregularRequestCount = 0;

        [HttpGet("fail/{userId}")]
        public async Task<IActionResult> Fail(string userId)
        {
            Logger.LogRequest(ActionType.Received, HttpMethod.Get, $"fail/{userId}");

            await Task.Delay(100); // simulate some data processing

            return ErrorResponse();
        }

        [HttpGet("irregular/{userId}")]
        public async Task<IActionResult> Irregular(string userId)
        {
            Logger.LogRequest(ActionType.Received, HttpMethod.Get, $"irregular/{userId}");

            await Task.Delay(100); // simulate some data processing

            var isFourthRequest = ++_irregularRequestCount % 4 == 0;

            return isFourthRequest
                ? OkResponse()
                : ErrorResponse();
        }

        [HttpGet("auth/{userId}")]
        public async Task<IActionResult> Auth(string userId)
        {
            Logger.LogRequest(ActionType.Received, HttpMethod.Get, $"auth/{userId}");

            await Task.Delay(100); // simulate some data processing

            var isAuthenticated = Request.Headers["Authorization"] == "Bearer fresh-token";

            return isAuthenticated
                ? OkResponse()
                : UnauthorizedResponse();
        }

        [HttpGet("slow/{userId}")]
        public async Task<IActionResult> Slow(string userId)
        {
            Logger.LogRequest(ActionType.Received, HttpMethod.Get, $"slow/{userId}");

            await Task.Delay(10000); // simulate some heavy data processing by delaying for 10 seconds

            return TimeoutResponse();
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

        private IActionResult SendResponse(HttpStatusCode statusCode, object content)
        {
            Logger.LogResponse(ActionType.Sending, statusCode, content);
            return StatusCode((int)statusCode, content);
        }

        private IActionResult OkResponse() => SendResponse(HttpStatusCode.OK, 15);

        private IActionResult ErrorResponse() => SendResponse(HttpStatusCode.InternalServerError, "Something went horribly wrong!");

        private IActionResult TimeoutResponse() => SendResponse(HttpStatusCode.RequestTimeout, "The request timed out.");

        private IActionResult UnauthorizedResponse() => SendResponse(HttpStatusCode.Unauthorized, "You are not authenticated.");

        #endregion
    }
}
