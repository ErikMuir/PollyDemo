using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using MuirDev.ConsoleTools;

namespace PollyDemo.App
{
    public class OLD_AppClient
    {
        private readonly HttpClient _httpClient;
        private readonly IDemo _withoutPollyDemo;
        private readonly IDemo _fallbackPolicyDemo;
        private readonly IDemo _retryPolicyDemo;
        private readonly IDemo _waitAndRetryPolicyDemo;
        private readonly IDemo _policyDelegatesDemo;
        private readonly IDemo _timeoutPolicyDemo;
        private readonly IDemo _policyWrappingDemo;
        private readonly IDemo _circuitBreakerFailsDemo;
        private readonly IDemo _circuitBreakerRecoversDemo;

        public OLD_AppClient(HttpClient client)
        {
            _httpClient = client;

            _withoutPollyDemo = new WithoutPolly(_httpClient);
            _fallbackPolicyDemo = new Fallback(_httpClient);
            _retryPolicyDemo = new Retry(_httpClient);
            _waitAndRetryPolicyDemo = new WaitAndRetry(_httpClient);
            _policyDelegatesDemo = new Delegates(_httpClient);
            _timeoutPolicyDemo = new Timeout(_httpClient);
            _policyWrappingDemo = new Wrap(_httpClient);
            _circuitBreakerFailsDemo = new CircuitBreakerFails(_httpClient);
            _circuitBreakerRecoversDemo = new CircuitBreakerRecovers(_httpClient);
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
                await Setup();
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
                    await _withoutPollyDemo.Run();
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

        private async Task Setup()
        {
            await _httpClient.GetAsync("/setup");
        }

        private void Shutdown()
        {
            _httpClient.GetAsync("/shutdown");
        }
    }
}