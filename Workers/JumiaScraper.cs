using FourtitudeIntegrated.DbContexts;
using HtmlAgilityPack;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Prema.PriceHarborScraper.AppSettings;
using Prema.PriceHarborScraper.Models;
using Prema.PriceHarborScraper.Policies;
using Prema.PriceHarborScraper.Repository;
using Prema.PriceHarborScraper.AppSettings;
using PuppeteerSharp;
using Serilog;

namespace Prema.PriceHarborScraper.Workers
{
    public class JumiaScraper : BackgroundService
    {
        private readonly ILogger<JumiaScraper> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly PollyPolicy _polly;
        private readonly Settings _appSettings;
        public JumiaScraper(ILogger<JumiaScraper> logger, IServiceProvider serviceProvider, PollyPolicy polly, IOptionsMonitor<Settings> appSettings)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _polly = polly;
            _appSettings = appSettings.CurrentValue;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Jumia Web Sraper running at: {time}", DateTimeOffset.Now);

                await Scraper();

                _logger.LogInformation("Jumia Web Sraper complete at: {time}", DateTimeOffset.Now);

                await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
            }
        }

        public async Task Scraper()
        {
            var proxyServers = _appSettings.ProxyServers;

            var proxyServerArgument = string.Join(",", proxyServers);
            int proxyCount = 0;
            bool complete = false;

            while (!complete)
            {

                var launchOptions = new LaunchOptions
                {
                    Headless = true,
                    Args = new[] { $"--proxy-server={proxyServers[proxyCount]}" }
                };

                _logger.LogInformation("Fetching browser.");
                await new BrowserFetcher().DownloadAsync();
                _logger.LogInformation("Launching browser.");
                using var browser = await Puppeteer.LaunchAsync(launchOptions);
                _logger.LogInformation("Launching new page.");
                using var page = await browser.NewPageAsync();

                string baseUrl = "https://www.jumia.co.ke";
                string categoryUrl = "/smartphones/";
                string url = baseUrl + categoryUrl;
                List<Product> products = new List<Product>();
                bool validPage = true;
                int pageCount = 1;
                int deviceCount = 1;

                while (validPage && !complete)
                {
                    _logger.LogInformation($"Opening new link. Page number: {pageCount}");
                    //await page.GoToAsync($"{url}?page={pageCount}#catalog-listing");
                    try
                    {
                        await _polly.LinearHttpRetry.ExecuteAsync(async () =>
                        {
                            try
                            {
                                await page.GoToAsync($"{url}?page={pageCount}#catalog-listing");                    
                                // Wait for the page to load, you can adjust the delay as needed
                                await page.WaitForTimeoutAsync(3000);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError($"GoToAsync: {ex.Message}");
                                throw;
                            }
                        });
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error: {ex.Message}");
                        _logger.LogError($"Problem with proxy {proxyServers[proxyCount]}, trying new proxy {proxyServers[proxyCount == proxyServers.Count - 1 ? 0 : proxyCount + 1]}.");
                        proxyCount = proxyCount == proxyServers.Count - 1 ? 0 : proxyCount + 1; 
                        continue;
                    }


                    var divNode = await page.QuerySelectorAsync("h2.-pvs.-fs16.-m");

                    if (divNode != null)
                    {
                        var divText = await divNode.EvaluateFunctionAsync<string>("el => el.innerText");
                        validPage = divText.Trim().Equals("No results found!", StringComparison.OrdinalIgnoreCase) ? false : true;

                        if (!validPage) continue;
                    }

                    var targetDivClass = "-paxs.row._no-g._4cl-3cm-shs";
                    var div = await page.QuerySelectorAsync($"div.{targetDivClass}");

                    pageCount++;

                    if (div != null)
                    {
                        var links = await div.QuerySelectorAllAsync("a");

                        _logger.LogInformation("Processing data.");
                        foreach (var link in links)
                        {
                            var href = await link.EvaluateFunctionAsync<string>("el => el.getAttribute('href')");
                            var product = new Product
                            {
                                Name = href,
                                PlatformId = 1,
                                ManufacturerId = 1,
                                ProductGroupId = 1,
                                Link = baseUrl + href
                            };
                            products.Add(product);
                            Console.WriteLine(href);
                            deviceCount++;
                        }
                    }
                    else
                    {
                        _logger.LogWarning("Div with class not found.");
                    }

                    // You can adjust the delay as needed
                    await Task.Delay(600);
                }

                _logger.LogInformation($"Match manufacturers.");
                GetManufacturer(products);

                using (var scope = _serviceProvider.CreateScope())
                {
                    var scopedProductRepository = scope.ServiceProvider.GetRequiredService<IRepository<Product>>();
                    scopedProductRepository.AddList(products);
                }
                _logger.LogInformation($"Total Items Found: {deviceCount}");
                complete = true;
            }
        }

        public void GetManufacturer(List<Product> products)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var scopedProductRepository = scope.ServiceProvider.GetRequiredService<IRepository<Manufacturer>>();
                var manufacturers = scopedProductRepository.GetAll();

                foreach (var manufacturer in manufacturers)
                {
                    foreach (var product in products.Where(x => x.ManufacturerId == 1).ToList())
                    {
                        if (product.Name.IndexOf(manufacturer.ManufacturerName, StringComparison.OrdinalIgnoreCase) != -1)
                        {
                            product.ManufacturerId = manufacturer.ManufacturerId;
                        }
                    }
                }
            }
        }
    }
}