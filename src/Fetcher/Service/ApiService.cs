using Fetcher.DTOs;
using Microsoft.Extensions.Logging;
using System.Text;

namespace Fetcher.Service;

internal sealed class ApiService : IApiService
{
    private static string DATA_PATH = "/random?auth=null";

    private readonly ILogger<ApiService> _logger;
    private readonly HttpClient _httpClient;

    public ApiService(HttpClient httpClient, ILogger<ApiService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<Result<Payload>> GetDataAsync(CancellationToken ct)
    {
        _logger.LogDebug($"Fetching data from API: {DATA_PATH}");

        HttpResponseMessage result = await _httpClient.GetAsync(DATA_PATH, ct);

        string resultContent = await result.Content.ReadAsStringAsync();

        if (result.IsSuccessStatusCode == false)
        {
            return Result<Payload>.Failure($"Reason: {result.ReasonPhrase}\nDetails: {resultContent}" ?? "Unknown");
        }

        return Result<Payload>.Success(new Payload(Encoding.UTF8.GetBytes(resultContent)));
    }
}
