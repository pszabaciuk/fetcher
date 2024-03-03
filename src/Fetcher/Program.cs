using Azure.Core.Diagnostics;
using Fetcher.Options;
using Fetcher.Persistence;
using Fetcher.Service;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureAppConfiguration((ctx, configBuilder) =>
    {
        configBuilder.AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables();
    })
    .ConfigureServices((ctx, services) =>
    {
        services.AddScoped<IDataFetcherService, DataFetcherService>();
        services.AddScoped<IPayloadPersistence, PayloadPersistence>();
        services.AddScoped<ILogPersistence, LogPersistence>();
        services.AddScoped(_ => new AzureStorageConfig { ConnectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage") ?? "" });
        services.AddHttpClient<IApiService, ApiService>(client => client.BaseAddress = new Uri(Environment.GetEnvironmentVariable("ApiBaseUrl") ?? ""));
    })
    .Build();

using AzureEventSourceListener listener = AzureEventSourceListener.CreateConsoleLogger();

await host.RunAsync();
