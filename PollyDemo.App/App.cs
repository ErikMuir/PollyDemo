using PollyDemo.App.Demos;
using PollyDemo.Common;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;

namespace PollyDemo.App
{
    public class App
    {
        private HttpClient _httpClient;
        private readonly BeforePollyDemo _beforePolicyDemo = new BeforePollyDemo();
        private readonly FallbackPolicyDemo _fallbackPolicyDemo = new FallbackPolicyDemo();
        private readonly RetryPolicyDemo _retryPolicyDemo = new RetryPolicyDemo();
        private readonly WaitAndRetryPolicyDemo _waitAndRetryPolicyDemo = new WaitAndRetryPolicyDemo();
        private readonly PolicyDelegatesDemo _policyDelegatesDemo = new PolicyDelegatesDemo();
        private readonly TimeoutPolicyDemo _timeoutPolicyDemo = new TimeoutPolicyDemo();
        private readonly PolicyWrappingDemo _policyWrappingDemo = new PolicyWrappingDemo();
        private readonly CircuitBreakerFailsDemo _circuitBreakerFailsDemo = new CircuitBreakerFailsDemo();
        private readonly CircuitBreakerRecoversDemo _circuitBreakerRecoversDemo = new CircuitBreakerRecoversDemo();

        public App()
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri(Constants.BaseAddress);
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public void Run()
        {
            while (true)
            {
                Clear();
                ShowMenu();
                var response = GetResponse();
                Console.WriteLine();
                switch (response)
                {
                    case "1":
                        _beforePolicyDemo.Run().Wait();
                        break;
                    case "2":
                        _fallbackPolicyDemo.Run().Wait();
                        break;
                    case "3":
                        _retryPolicyDemo.Run().Wait();
                        break;
                    case "4":
                        _waitAndRetryPolicyDemo.Run().Wait();
                        break;
                    case "5":
                        _policyDelegatesDemo.Run().Wait();
                        break;
                    case "6":
                        _timeoutPolicyDemo.Run().Wait();
                        break;
                    case "7":
                        _policyWrappingDemo.Run().Wait();
                        break;
                    case "8":
                        _circuitBreakerFailsDemo.Run().Wait();
                        break;
                    case "9":
                        _circuitBreakerRecoversDemo.Run().Wait();
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
        }

        private string GetResponse()
        {
            var allowedResponses = new List<string> { "1", "2", "3", "4", "5", "6", "7", "8", "9", "q" };
            var response = string.Empty;
            Console.WriteLine();
            while (!allowedResponses.Contains(response))
            {
                Console.Write("Choose a demo (or 'q' to quit): ");
                response = Console.ReadLine().Trim().ToLower();
            }
            return response;
        }

        private void Continue()
        {
            Console.WriteLine();
            Console.Write("Press Enter to continue... ");
            while (Console.ReadKey(true).Key != ConsoleKey.Enter) { }
        }

        private async void Clear()
        {
            await _httpClient.GetAsync("clear");
        }

        private async void Shutdown()
        {
            await _httpClient.GetAsync("shutdown");
        }
    }
}
