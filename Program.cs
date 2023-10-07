using FourtitudeIntegrated.DbContexts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using Prema.PriceHarborScraper.Workers;
using Prema.PriceHarborScraper.Repository;
using Prema.PriceHarborScraper.Policies;

IHost host = Host.CreateDefaultBuilder(args)
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
    .Build();

await host.RunAsync();
