using Azure;
using Azure.Data.Tables;
using Fetcher.DTOs;
using Fetcher.Options;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Fetcher.Persistence;

internal sealed class LogPersistence : ILogPersistence
{
    private static string TABLE_NAME = "logs";
    private static string PATH_COLUMN_NAME = "Path";
    private static string WAS_SUCCESS_COLUMN_NAME = "WasSuccess";
    private static string CREATION_DATE_COLUMN_NAME = "CreationDate";
    private static string ERROR_MESSAGE_COLUMN_NAME = "ErrorMessage";

    private readonly TableClient _tableClient;
    private readonly ILogger<LogPersistence> _logger;

    public LogPersistence(AzureStorageConfig config, ILogger<LogPersistence> logger)
    {
        _logger = logger;

        _tableClient = new TableClient(config.ConnectionString, TABLE_NAME);
    }

    public async Task<Result<Empty>> AddLogAsync(Status status, CancellationToken ct)
    {
        string id = status.Id.ToString(); // for now, we assume, there is no need to scalability so we have partition key == row key

        _logger.LogDebug($"Creating log entry {id} in table {TABLE_NAME}");

        await _tableClient.CreateIfNotExistsAsync(ct);

        var tableEntity = new TableEntity(id, id)
        {
            { PATH_COLUMN_NAME, status.Path },
            { WAS_SUCCESS_COLUMN_NAME, status.WasSuccess },
            { CREATION_DATE_COLUMN_NAME, status.CreationDate },
            { ERROR_MESSAGE_COLUMN_NAME, status.ErrorMessage }
        };

        Response result = await _tableClient.AddEntityAsync(tableEntity, ct);

        if (result.IsError)
        {
            return Result<Empty>.Failure($"Reason: {result.ReasonPhrase}");
        }

        return Result<Empty>.Success(null);
    }

    public async Task<Result<string>> GetLogPathAsync(Guid logId, CancellationToken ct)
    {
        string id = logId.ToString(); // for now, we assume, there is no need to scalability so we have partition key == row key

        Response<TableEntity> response = await _tableClient.GetEntityAsync<TableEntity>(id, id, cancellationToken: ct);

        if (response.HasValue == false)
        {
            return Result<string>.Failure($"There is no log with id {id}");
        }

        return Result<string>.Success(response.Value.GetString(PATH_COLUMN_NAME));
    }

    public async Task<Result<string>> GetLogsAsync(DateTime from, DateTime to, CancellationToken ct)
    {
        Pageable<TableEntity> queryResultsFilter = _tableClient.Query<TableEntity>(filter: $"CreationDate ge datetime'{from.ToString("o")}' and CreationDate le datetime'{to.ToString("o")}'", cancellationToken: ct);

        IEnumerable<Status> result = queryResultsFilter.Select(s =>
            new Status(Guid.Parse(s.RowKey), s.GetString(PATH_COLUMN_NAME), s.GetBoolean(WAS_SUCCESS_COLUMN_NAME)!.Value, s.GetDateTime(CREATION_DATE_COLUMN_NAME)!.Value)
            {
                ErrorMessage = s.GetString(ERROR_MESSAGE_COLUMN_NAME)
            });

        string resultJson = JsonSerializer.Serialize(result);

        return Result<string>.Success(resultJson);
    }
}
