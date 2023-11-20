using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using Prema.PriceHarbor.Scraper.Workers;
using Prema.PriceHarbor.Scraper.Repository;
using Prema.PriceHarbor.Scraper.Policies;
using Serilog;
using Serilog.Events;
using Prema.PriceHarbor.Scraper.AppSettings;
using MassTransit;
using System.Configuration;
using Prema.PriceHarbor.Scraper.DbContexts;
using Prema.PriceHarbor.Contracts;

public class Program
{
    public static async Task<int> Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .WriteTo.File("logs/Scrapper.log", rollingInterval: RollingInterval.Day)            
            .CreateLogger();    

        try
        {
            Log.Information("Starting host");
            var host = BuildHost(args);
            await host.RunAsync();
            return 0;
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Host terminated unexpectedly");
            return 1;
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    public static IHost BuildHost(string[] args) =>
        new HostBuilder()
            .ConfigureAppConfiguration((hostContext, configBuilder) =>
            {
                configBuilder
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true);
            })
            .ConfigureServices((hostContext, services) =>
            {
                // Add MySQL DbContext
                var connectionString = hostContext.Configuration.GetConnectionString("MySql");
                services.AddDbContextPool<PriceHarborContext>(options => options.UseMySQL(connectionString));

                services.Configure<Settings>(hostContext.Configuration.GetSection("AppSettings"));
                var rabbitMqOptions = hostContext.Configuration.GetSection("AppSettings:RabbitMqOptions").Get<RabbitMqOptions>();

                services.AddMassTransit(x =>
                {
                    x.UsingRabbitMq((context, cfg) =>
                    {
                        cfg.Host(rabbitMqOptions.HostName, rabbitMqOptions.Port, rabbitMqOptions.VHost, h =>
                        {
                            h.Username(rabbitMqOptions.UserName);
                            h.Password(rabbitMqOptions.Password);
                        });

                        cfg.ReceiveEndpoint("new-product-data-found", e => 
                        {
                            e.Bind<ProductData>();
                        });

                        cfg.Publish<ProductData>(x =>
                        {
                            x.Durable = true;
                        });

                    });
                });

                //services.AddTransient<IRepository, Repository>();
                services.AddSingleton<PollyPolicy>();
                services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
                services.AddHostedService<JumiaScraper>();

            })
            .UseSerilog() // <- Add this line
            .Build();

}