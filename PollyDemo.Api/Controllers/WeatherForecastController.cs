using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace PollyDemo.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static int _failCount = 0;
        private static int _simulateDataProcessing = 250;
        private static int _simulateHangingService = 5000;
        private static readonly string[] _summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };
        private readonly IApiLogger _logger;

        public WeatherForecastController(IApiLogger logger)
        {
            _logger = logger;
        }

        [HttpGet("/")]
        public async Task<IActionResult> Get()
        {
            _logger.LogRequest(Request);
            await Task.Delay(_simulateDataProcessing);
            return await SendResponse(HttpStatusCode.OK, GetForecast());
        }

        [HttpGet("/fail/{*count}")]
        public async Task<IActionResult> Fail(int count)
        {
            _logger.LogRequest(Request);
            await Task.Delay(_simulateDataProcessing);
            return count > 0 && ++_failCount > count
                ? await SendResponse(HttpStatusCode.OK, GetForecast())
                : await SendResponse(HttpStatusCode.InternalServerError);
        }

        [HttpGet("/bad-request")]
        public async Task<IActionResult> Bad()
        {
            _logger.LogRequest(Request);
            await Task.Delay(_simulateDataProcessing);
            return await SendResponse(HttpStatusCode.BadRequest);
        }

        [HttpGet("/slow")]
        public async Task<IActionResult> Slow()
        {
            _logger.LogRequest(Request);
            await Task.Delay(_simulateDataProcessing);
            await Task.Delay(_simulateHangingService);
            return await SendResponse(HttpStatusCode.OK, GetForecast());
        }

        [HttpGet("/auth")]
        public async Task<IActionResult> Auth()
        {
            _logger.LogRequest(Request);
            await Task.Delay(_simulateDataProcessing);
            return Request.Headers["Authorization"] == "Bearer fresh-token"
                ? await SendResponse(HttpStatusCode.OK, GetForecast())
                : await SendResponse(HttpStatusCode.Unauthorized);
        }

        [HttpGet("/timeout/{*count}")]
        public async Task<IActionResult> Timeout(int count)
        {
            _logger.LogRequest(Request);
            await Task.Delay(_simulateDataProcessing);
            if (count > 0 && ++_failCount > count)
                return await SendResponse(HttpStatusCode.OK, GetForecast());
            await Task.Delay(_simulateHangingService);
            return await SendResponse(HttpStatusCode.RequestTimeout);
        }

        [HttpGet("/setup")]
        public IActionResult Setup()
        {
            _logger.Clear();
            _failCount = 0;
            return base.Ok();
        }

        private string GetForecast() => _summaries[new Random().Next(_summaries.Length)];

        private async Task<IActionResult> SendResponse(HttpStatusCode statusCode, string content = null)
        {
            _logger.LogResponse(statusCode, content);
            await Task.Delay(250);
            return StatusCode((int)statusCode, content);
        }
    }
}
