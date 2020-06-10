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
            var path = ComposePath(args);
            var services = new ServiceCollection();

            // var retryPolicy = HttpPolicyExtensions
            //     .HandleTransientHttpError()
            //     .Or<TimeoutRejectedException>() // thrown by Polly's TimeoutPolicy if the inner execution times out
            //     .RetryAsync(3);

            // var timeoutPolicy = Policy.TimeoutAsync<HttpResponseMessage>(10);

            services
                .AddHttpClient<App>(x =>
                {
                    x.BaseAddress = new Uri("http://localhost:5000/api/WeatherForecast");
                    x.DefaultRequestHeaders.Accept.Clear();
                    x.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                })
                // .AddPolicyHandler(retryPolicy)
                // .AddPolicyHandler(timeoutPolicy)
                ;

            var serviceProvider = services.BuildServiceProvider();
            var app = serviceProvider.GetRequiredService<App>();

            await app.Run(path);
        }

        private static string ComposePath(string[] args)
        {
            var path = "/";

            if (args.Length > 0)
                path += args[0].ToLower();

            if (args.Length > 1 && (path == "/fail" || path == "/timeout"))
            {
                int.TryParse(args[1], out var count);
                path += $"/{count}";
            }

            return path;
        }
    }
}
