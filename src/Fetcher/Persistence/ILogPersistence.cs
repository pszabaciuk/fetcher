using Fetcher.DTOs;

namespace Fetcher.Persistence;

public interface ILogPersistence
{
    Task<Result<Empty>> AddLogAsync(Status status, CancellationToken ct);
    Task<Result<string>> GetLogsAsync(DateTime from, DateTime to, CancellationToken ct);
    Task<Result<string>> GetLogPathAsync(Guid logId, CancellationToken ct);
}