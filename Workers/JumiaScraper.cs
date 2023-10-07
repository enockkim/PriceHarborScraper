using FourtitudeIntegrated.DbContexts;
using HtmlAgilityPack;
using Microsoft.Extensions.DependencyInjection;
using Prema.PriceHarborScraper.Models;
using Prema.PriceHarborScraper.Policies;
using Prema.PriceHarborScraper.Repository;
using PuppeteerSharp;

namespace Prema.PriceHarborScraper.Workers
{
    public class JumiaScraper : BackgroundService
    {
        private readonly ILogger<JumiaScraper> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly PollyPolicy _polly;
        public JumiaScraper(ILogger<JumiaScraper> logger, IServiceProvider serviceProvider, PollyPolicy polly)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _polly = polly;
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

        //void Srcaper()
        //{
        //    // Send get request to jumia
        //    //string url = "https://www.jumia.co.ke/samsung-galaxy-a04s-6.5-128gb-4gb-ram-dual-sim-50mp-5000mah-black-123460625.html";
        //    string baseUrl = "https://www.jumia.co.ke";
        //    string categoryUrl = "/smartphones/";
        //    string url = baseUrl + categoryUrl;
        //    var httpClient = new HttpClient();
        //    List<Product> products = new List<Product>();

        //    //// Get temp
        //    //var itemElement = htmlDocument.DocumentNode.SelectSingleNode("//h1[@class='-fs20 -pts -pbxs']");
        //    //var item = itemElement.InnerText.Trim();
        //    //Console.WriteLine($"Item: {item}");

        //    //            // Get temp
        //    //var priceElement = htmlDocument.DocumentNode.SelectSingleNode("//span[@class='-b -ltr -tal -fs24 -prxs']");
        //    //var price = priceElement.InnerText.Trim();
        //    //Console.WriteLine($"Price: {price}");
        //    bool validPage = true;
        //    int pageCount = 1;
        //    int deviceCount = 1;

        //    while (validPage)
        //    {
        //        var html = httpClient.GetStringAsync($"{url}?page={pageCount}#catalog-listing").Result;
        //        var htmlDocument = new HtmlDocument();
        //        htmlDocument.LoadHtml(html);

        //        //check if page has a result hence is valid
        //        var divNode = htmlDocument.DocumentNode.SelectSingleNode($"//h2[@class='-pvs -fs16 -m']");

        //        if (divNode != null)
        //        {
        //            validPage = divNode.InnerText.Trim().Equals("No results found!") ? false : true;

        //            if (!validPage) continue;
        //        }

        //        // Specify the class name of the div you want to target
        //        string targetDivClass = "-paxs row _no-g _4cl-3cm-shs";

        //        // Find the div element with the specified class
        //        divNode = htmlDocument.DocumentNode.SelectSingleNode($"//div[@class='{targetDivClass}']");

        //        pageCount++;

        //        if (divNode != null)
        //        {
        //            // Find all <a> elements within the div
        //            var hrefAttributes = divNode.Descendants("a")
        //                .Select(a => a.GetAttributeValue("href", string.Empty))
        //                .ToList();

        //            // Print the href attributes
        //            foreach (var href in hrefAttributes)
        //            {
        //                var product = new Product()
        //                {
        //                    Name = href,
        //                    PlatformId = 1,
        //                    ManufacturerId = 1,
        //                    ProductGroupId = 1,
        //                    Link = baseUrl + href
        //                };
        //                products.Add(product);
        //                Console.WriteLine(href);
        //                deviceCount++;
        //            }
        //        }
        //        else
        //        {
        //            Console.WriteLine("Div with class not found.");
        //        }
        //        Thread.Sleep(600);
                   
        //    }

        //    GetManufacturer(products);

        //    using (var scope = _serviceProvider.CreateScope())  
        //    {
        //            var scopedProductRepository = scope.ServiceProvider.GetRequiredService<IRepository<Product>>();
        //        scopedProductRepository.AddList(products);
        //    }
        //    Console.WriteLine($"Total Items Found: {deviceCount}");
        //}

        public async Task Scraper()
        {
            var proxyServers = new[]
                {
                    "86.96.15.70:8080",
                    "202.154.36.57:8080",
                    "179.42.9.242:3128",
                    "196.251.134.147:8080",
                    "117.71.149.66:8089",
                    "207.2.120.19:80",
                    "45.171.108.6:999",
                    "64.225.8.142:10006",
                    "133.242.229.79:33333",
                    "34.82.224.175:33333",
                    "188.166.17.18:8881",
                    "124.167.20.48:7777"
                };

            var proxyServerArgument = string.Join(",", proxyServers);

            var launchOptions = new LaunchOptions
            {
                Headless = true,
                Args = new[] { $"--proxy-server={proxyServerArgument}" }
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

            while (validPage)
            {
                _logger.LogInformation("Opening new link.");
                //await page.GoToAsync($"{url}?page={pageCount}#catalog-listing");
                await _polly.LinearHttpRetry.ExecuteAsync(async () => 
                {
                    try 
                    {
                        await page.GoToAsync($"{url}?page={pageCount}#catalog-listing");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"GoToAsync: {ex.Message}");
                        throw;
                    }
                });

                // Wait for the page to load, you can adjust the delay as needed
                await page.WaitForTimeoutAsync(3000);

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