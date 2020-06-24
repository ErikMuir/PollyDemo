using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;
using Polly.Timeout;

namespace PollyDemo.App
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            var path = Helpers.ComposePath(args);
            var services = new ServiceCollection();

            var retryPolicy = HttpPolicyExtensions
                .HandleTransientHttpError()
                .Or<TimeoutRejectedException>()
                .RetryAsync(3);

            var timeoutPolicy = Policy.TimeoutAsync<HttpResponseMessage>(10);

            services
                .AddHttpClient<App>(x =>
                {
                    x.BaseAddress = new Uri("http://localhost:5000/api/WeatherForecast");
                    x.DefaultRequestHeaders.Accept.Clear();
                    x.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                })
                .AddPolicyHandler(retryPolicy)
                .AddPolicyHandler(timeoutPolicy);

            var serviceProvider = services.BuildServiceProvider();
            var app = serviceProvider.GetRequiredService<App>();

            await app.Run(path);
        }
    }
}
