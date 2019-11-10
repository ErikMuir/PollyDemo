using PollyDemo.App.Demos;
using PollyDemo.Common;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

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

        public async Task Run()
        {
            while (true)
            {
                await Clear();
                switch (GetResponse())
                {
                    case "1":
                        await _beforePolicyDemo.Run();
                        break;
                    case "2":
                        await _fallbackPolicyDemo.Run();
                        break;
                    case "3":
                        await _retryPolicyDemo.Run();
                        break;
                    case "4":
                        await _waitAndRetryPolicyDemo.Run();
                        break;
                    case "5":
                        await _policyDelegatesDemo.Run();
                        break;
                    case "6":
                        await _timeoutPolicyDemo.Run();
                        break;
                    case "7":
                        await _policyWrappingDemo.Run();
                        break;
                    case "8":
                        await _circuitBreakerFailsDemo.Run();
                        break;
                    case "9":
                        await _circuitBreakerRecoversDemo.Run();
                        break;
                    case "q":
                        Shutdown();
                        Environment.Exit(0);
                        break;
                }
                Continue();
            }
        }

        private void ShowMenu()
        {
            Console.Clear();
            Console.WriteLine("********** Demos **********");
            Console.WriteLine(" 1 - Without Polly");
            Console.WriteLine(" 2 - Fallback Policy");
            Console.WriteLine(" 3 - Retry Policy");
            Console.WriteLine(" 4 - Wait and Retry Policy");
            Console.WriteLine(" 5 - Policy Delegates");
            Console.WriteLine(" 6 - Timeout Policy");
            Console.WriteLine(" 7 - Policy Wrapping (Fallback, Retry, Timeout)");
            Console.WriteLine(" 8 - Circuit Breaker Policy (fails)");
            Console.WriteLine(" 9 - Circuit Breaker Policy (recovers)");
            Console.WriteLine();
        }

        private string GetResponse()
        {
            var allowedResponses = new List<string> { "1", "2", "3", "4", "5", "6", "7", "8", "9", "q" };
            var response = string.Empty;
            ShowMenu();
            while (!allowedResponses.Contains(response))
            {
                Console.Write("Choose a demo (or 'q' to quit): ");
                response = Console.ReadLine().Trim().ToLower();
            }
            Console.WriteLine();
            return response;
        }

        private void Continue()
        {
            Console.WriteLine();
            Console.Write("Press Enter to continue... ");
            while (Console.ReadKey(true).Key != ConsoleKey.Enter) { }
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
