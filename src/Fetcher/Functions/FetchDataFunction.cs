using Fetcher.Service;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Fetcher.Functions;

public sealed class FetchDataFunction
{
    private readonly IDataFetcherService _dataFetcherService;
    private readonly ILogger<FetchDataFunction> _logger;

    public FetchDataFunction(IDataFetcherService dataFetcherService, ILogger<FetchDataFunction> logger)
    {
        _dataFetcherService = dataFetcherService;
        _logger = logger;
    }

    [Function("FetchDataFunction")]
    public async Task Run([TimerTrigger("0 */1 * * * *", RunOnStartup = false)] TimerInfo myTimer, CancellationToken ct)
    {
        _logger.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

        try
        {
            await _dataFetcherService.FetchDataAsync(ct);
        }
        catch (Exception ex)
        {
            _logger.LogError($"There was an error while executing function: {ex.Message}");
        }

        if (myTimer.ScheduleStatus is not null)
        {
            _logger.LogInformation($"C# Timer trigger function ended at: {DateTime.Now}");
            _logger.LogInformation($"Next timer schedule at: {myTimer.ScheduleStatus.Next}");
        }
    }
}
