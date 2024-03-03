namespace Fetcher.Service;

public interface IDataFetcherService
{
    Task FetchDataAsync(CancellationToken ct);
}