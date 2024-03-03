using Fetcher.DTOs;

namespace Fetcher.Persistence;

public interface IPayloadPersistence
{
    Task<Result<string>> AddPayloadAsync(Payload value, CancellationToken ct);

    Task<Result<string>> GetPayloadAsync(string path, CancellationToken ct);
}