using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Fetcher.DTOs;
using Fetcher.Extensions;
using Fetcher.Options;
using Microsoft.Extensions.Logging;

namespace Fetcher.Persistence;

internal sealed class PayloadPersistence : IPayloadPersistence
{
    private static string CONTAINER_NAME = "payloads";

    private readonly BlobServiceClient _blobServiceClient;
    private readonly ILogger<PayloadPersistence> _logger;

    public PayloadPersistence(AzureStorageConfig config, ILogger<PayloadPersistence> logger)
    {
        _logger = logger;

        _blobServiceClient = new BlobServiceClient(config.ConnectionString);
    }

    public async Task<Result<string>> AddPayloadAsync(Payload value, CancellationToken ct)
    {
        string blobName = GeneratePath();

        _logger.LogDebug($"Creating payload blob: {blobName}");

        BlobContainerClient container = _blobServiceClient.GetBlobContainerClient(CONTAINER_NAME);
        await container.CreateIfNotExistsAsync(cancellationToken: ct);

        BlobClient blob = container.GetBlobClient(blobName);
        Response<BlobContentInfo> result = await blob.UploadAsync(new MemoryStream(value.Bytes));

        if (result.GetRawResponse().IsError == true)
        {
            return Result<string>.Failure($"Reason: {result.GetRawResponse().ReasonPhrase}");
        }

        return Result<string>.Success($"{CONTAINER_NAME}/{blobName}");
    }

    public async Task<Result<string>> GetPayloadAsync(string path, CancellationToken ct)
    {
        _logger.LogDebug($"Retrieving payload blob: {path}");

        BlobContainerClient container = _blobServiceClient.GetBlobContainerClient(CONTAINER_NAME);
        if (container.Exists() == false)
        {
            return Result<string>.Failure($"There is no container {CONTAINER_NAME}");
        }

        BlobClient blob = container.GetBlobClient(path.Replace(CONTAINER_NAME, ""));
        if (blob.Exists() == false)
        {
            return Result<string>.Failure($"There is no blob in path {path}");
        }

        MemoryStream stream = new MemoryStream();
        await blob.DownloadToAsync(stream, ct);

        string result = await stream.ConvertToString();

        return Result<string>.Success(result);
    }

    private string GeneratePath() => DateTime.UtcNow.ToString(@"yyyy\/MM\/dd\/HH\/mm.j\s\on");
}
