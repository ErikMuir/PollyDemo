using System;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace PollyDemo.App
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            var services = new ServiceCollection();

            services.AddHttpClient<App>(x =>
            {
                x.BaseAddress = new Uri("http://localhost:5000/api/WeatherForecast");
                x.DefaultRequestHeaders.Accept.Clear();
                x.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            });

            var serviceProvider = services.BuildServiceProvider();
            var app = serviceProvider.GetRequiredService<App>();

            await app.Run();
        }
    }
}
