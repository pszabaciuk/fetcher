using Fetcher.DTOs;
using Fetcher.Persistence;
using Microsoft.Extensions.Logging;

namespace Fetcher.Service;

public sealed class DataFetcherService : IDataFetcherService
{
    private readonly IApiService _apiService;
    private readonly ILogPersistence _logPersistence;
    private readonly IPayloadPersistence _payloadPersistence;
    private readonly ILogger<DataFetcherService> _logger;

    public DataFetcherService(IApiService apiService, ILogPersistence logPersistence, IPayloadPersistence payloadPersistence, ILogger<DataFetcherService> logger)
    {
        _apiService = apiService;
        _logPersistence = logPersistence;
        _payloadPersistence = payloadPersistence;
        _logger = logger;
    }

    public async Task FetchDataAsync(CancellationToken ct)
    {
        Result<Payload> result = await _apiService.GetDataAsync(ct);

        if (result.IsSuccess == false)
        {
            _logger.LogError($"There was an error while receiving data from API: {result.Error}");
            return;
        }

        if (result.Value == null)
        {
            _logger.LogError($"There was an error while receiving data from API: there was no payload.");
            return;
        }

        Result<string> pathResult = await _payloadPersistence.AddPayloadAsync(result.Value, ct);

        if (pathResult.IsSuccess == false)
        {
            _logger.LogError($"There was an error while adding payload: {result.Error}");
            return;
        }

        Status status = new Status(Guid.NewGuid(), pathResult.Value, result.IsSuccess, DateTime.UtcNow)
        {
            ErrorMessage = result.Error
        };

        Result<Empty> logResult = await _logPersistence.AddLogAsync(status, ct);

        if (logResult.IsSuccess == false)
        {
            _logger.LogError($"There was an error while adding logs for payload: {result.Error}");
            return;
        }
    }
}
