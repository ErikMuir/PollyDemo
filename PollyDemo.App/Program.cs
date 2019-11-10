using System;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using PollyDemo.Common;

namespace PollyDemo.App
{
    public class Program
    {
        static async Task Main()
        {
            var services = new ServiceCollection();
            services.AddHttpClient<AppClient>(x =>
            {
                x.BaseAddress = new Uri(Constants.BaseAddress);
                x.DefaultRequestHeaders.Accept.Clear();
                x.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            });

            var serviceProvider = services.BuildServiceProvider();
            var app = serviceProvider.GetRequiredService<AppClient>();

            await app.Run();
        }
    }
}
