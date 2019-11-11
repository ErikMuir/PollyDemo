using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using MuirDev.ConsoleTools;
using PollyDemo.App.Demos;
using PollyDemo.Common;

namespace PollyDemo.App
{
    public class AppClient
    {
        private readonly HttpClient _httpClient;
        private readonly IDemo _beforePolicyDemo;
        private readonly IDemo _fallbackPolicyDemo;
        private readonly IDemo _retryPolicyDemo;
        private readonly IDemo _waitAndRetryPolicyDemo;
        private readonly IDemo _policyDelegatesDemo;
        private readonly IDemo _timeoutPolicyDemo;
        private readonly IDemo _policyWrappingDemo;
        private readonly IDemo _circuitBreakerFailsDemo;
        private readonly IDemo _circuitBreakerRecoversDemo;

        public AppClient(HttpClient client)
        {
            _httpClient = client;

            _beforePolicyDemo = new BeforePollyDemo(_httpClient);
            _fallbackPolicyDemo = new FallbackPolicyDemo(_httpClient);
            _retryPolicyDemo = new RetryPolicyDemo(_httpClient);
            _waitAndRetryPolicyDemo = new WaitAndRetryPolicyDemo(_httpClient);
            _policyDelegatesDemo = new PolicyDelegatesDemo(_httpClient);
            _timeoutPolicyDemo = new TimeoutPolicyDemo(_httpClient);
            _policyWrappingDemo = new PolicyWrappingDemo(_httpClient);
            _circuitBreakerFailsDemo = new CircuitBreakerFailsDemo(_httpClient);
            _circuitBreakerRecoversDemo = new CircuitBreakerRecoversDemo(_httpClient);
        }

        private static readonly IDictionary<char, string> _menuOptions = new Dictionary<char, string>
        {
            { '1', "Without Polly" },
            { '2', "Fallback Policy" },
            { '3', "Retry Policy" },
            { '4', "Wait and Retry Policy" },
            { '5', "Policy Delegates" },
            { '6', "Timeout Policy" },
            { '7', "Policy Wrapping" },
            { '8', "Circuit Breaker Policy (fails)" },
            { '9', "Circuit Breaker Policy (recovers)" },
            { 'q', "quit" },
        };

        private static readonly Menu _menu = new Menu(_menuOptions, "Demos");

        public async Task Run()
        {
            while (true)
            {
                await Clear();
                Console.Clear();
                var response = _menu.Run();
                if (response == 'q')
                {
                    Shutdown();
                    Environment.Exit(0);
                }
                await RunDemo(response);
                Continue();
            }
        }

        private async Task RunDemo(char menuOption)
        {
            switch (menuOption)
            {
                case '1':
                    await _beforePolicyDemo.Run();
                    break;
                case '2':
                    await _fallbackPolicyDemo.Run();
                    break;
                case '3':
                    await _retryPolicyDemo.Run();
                    break;
                case '4':
                    await _waitAndRetryPolicyDemo.Run();
                    break;
                case '5':
                    await _policyDelegatesDemo.Run();
                    break;
                case '6':
                    await _timeoutPolicyDemo.Run();
                    break;
                case '7':
                    await _policyWrappingDemo.Run();
                    break;
                case '8':
                    await _circuitBreakerFailsDemo.Run();
                    break;
                case '9':
                    await _circuitBreakerRecoversDemo.Run();
                    break;
            }
        }

        private void Continue()
        {
            Console.WriteLine();
            Console.Write("Press any key to continue... ");
            Console.ReadKey(true);
        }

        private async Task Clear()
        {
            await _httpClient.GetAsync("/clear");
        }

        private void Shutdown()
        {
            _httpClient.GetAsync("/shutdown");
        }
    }
}
