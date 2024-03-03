using Fetcher.DTOs;

namespace Fetcher.Service;

public interface IApiService
{
    Task<Result<Payload>> GetDataAsync(CancellationToken ct);
}