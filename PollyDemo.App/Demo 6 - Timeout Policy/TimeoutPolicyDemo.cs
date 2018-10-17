﻿using Newtonsoft.Json;
using Polly;
using Polly.Timeout;
using PollyDemo.Common;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace PollyDemo.App.Demos
{
    public class TimeoutPolicyDemo : IDemo
    {
        private HttpClient _httpClient;
        private readonly TimeoutPolicy _timeoutPolicy;

        public TimeoutPolicyDemo()
        {
            _timeoutPolicy = Policy.TimeoutAsync(5, TimeoutStrategy.Optimistic); // throws TimeoutRejectedException if timeout of 5 seconds is exceeded
        }

        public async Task Run()
        {
            Console.WriteLine("Demo 6 - Timeout Policy");

            _httpClient = GetHttpClient();

            Utils.WriteRequest(ActionType.Sending, HttpMethod.Get, Constants.SlowRequest);

            try
            {
                var response = await _timeoutPolicy.ExecuteAsync(async token =>
                    await _httpClient.GetAsync(Constants.SlowRequest, token),
                    CancellationToken.None);
                var content = null as object;

                if (response.IsSuccessStatusCode)
                    content = JsonConvert.DeserializeObject<int>(await response.Content.ReadAsStringAsync());
                else if (response.Content != null)
                    content = await response.Content.ReadAsStringAsync();

                Utils.WriteResponse(ActionType.Received, response.StatusCode, content);
            }
            catch (TimeoutRejectedException e)
            {
                Utils.WriteException(e);
            }
        }

        private HttpClient GetHttpClient()
        {
            var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(Constants.BaseAddress);
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            return httpClient;
        }
    }
}
