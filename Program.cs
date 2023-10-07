using FourtitudeIntegrated.DbContexts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using Prema.PriceHarborScraper.Workers;
using Prema.PriceHarborScraper.Repository;
using Prema.PriceHarborScraper.Policies;
using Serilog;
using Serilog.Events;

public class Program
{
    public static async Task<int> Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .Enrich.FromLogContext()
            .WriteTo.Console()
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
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            })
            .ConfigureServices((hostContext, services) =>
            {
                // Add MySQL DbContext
                var connectionString = hostContext.Configuration.GetConnectionString("MySql");
                services.AddDbContextPool<PriceHarborContext>(options => options.UseMySQL(connectionString));

                //services.AddTransient<IRepository, Repository>();
                services.AddSingleton<PollyPolicy>();
                services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
                services.AddHostedService<JumiaScraper>();

            })
            .UseSerilog() // <- Add this line
            .Build();

}