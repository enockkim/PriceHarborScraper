using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using Prema.PriceHarborScraper.Workers;
using PuppeteerSharp;

namespace Prema.PriceHarborScraper.Policies
{
    public class PollyPolicy
    {

        private readonly IConfiguration _configuration;
        private readonly ILogger<PollyPolicy> _logger;
        //private static MemoryCache _mCache = MemoryCache.Default;

        public AsyncRetryPolicy<HttpResponseMessage> ImmediateHttpRetry { get; }
        public AsyncRetryPolicy LinearHttpRetry { get; }
        public AsyncRetryPolicy<HttpResponseMessage> ExponentialHttpRetry { get; }

        public PollyPolicy(IConfiguration configuration, ILogger<PollyPolicy> logger)
        {
            _configuration = configuration;
            _logger = logger;
            try
            {
                ImmediateHttpRetry = Policy.HandleResult<HttpResponseMessage>(
                    res => !res.IsSuccessStatusCode)
                    .RetryAsync(10);

                LinearHttpRetry = Policy
                    .Handle<Exception>()
                    .WaitAndRetryAsync(3, retryAttempt =>
                    {
                        TimeSpan retryDelay = TimeSpan.FromSeconds(3);
                        _logger.LogWarning($"Retrying in {retryDelay.TotalSeconds} seconds (Retry Attempt {retryAttempt})");
                        return retryDelay;
                    });

                ExponentialHttpRetry = Policy.HandleResult<HttpResponseMessage>(
                    res => !res.IsSuccessStatusCode)
                    .WaitAndRetryAsync(5, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

            }
            catch (Exception ex)
            {
                _logger.LogError("Policy Error: " + ex.Message);
            }

        }
    }
}
