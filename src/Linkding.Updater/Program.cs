using Linkding.Updater;
using LinkdingUpdater;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

static void ConfigureServices(IServiceCollection services)
{
    // configure logging
    services.AddLogging(builder =>
    {
        builder.AddSerilog();
        builder.AddConsole();
        builder.AddDebug();
    });

    // build config
    var configuration = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", optional: false)
        .AddUserSecrets<Program>(true)
        .AddEnvironmentVariables()
        .Build();

    // add services:
    services.Add_Linkding_HttpClient(configuration);

    // add app
    services.AddTransient<App>();
}


// create service collection
var services = new ServiceCollection();
ConfigureServices(services);

// create service provider
using var serviceProvider = services.BuildServiceProvider();

// entry to run app
await serviceProvider.GetService<App>().RunHandler(args);