using Fetcher.DTOs;
using Fetcher.Persistence;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;

namespace Fetcher.Functions;

public sealed class GetPayloadFunction : FunctionBase
{
    protected record Parameters : ParametersBase
    {
        public Guid Id { get; set; }
    }

    private readonly ILogger _logger;
    private readonly ILogPersistence _logPersistence;
    private readonly IPayloadPersistence _payloadPersistence;

    public GetPayloadFunction(ILogPersistence logPersistence, IPayloadPersistence payloadPersistence, ILoggerFactory loggerFactory)
    {
        _logPersistence = logPersistence;
        _payloadPersistence = payloadPersistence;
        _logger = loggerFactory.CreateLogger<GetPayloadFunction>();
    }

    [Function("GetPayloadFunction")]
    public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "payloads/{id}")] HttpRequestData req, string id, CancellationToken ct)
    {
        HttpResponseData response;

        _logger.LogInformation("C# HTTP trigger function processed a request.");

        try
        {
            Parameters parameters = ValidateParameters(id);
            if (parameters.ErrorMessage != null)
            {
                _logger.LogError($"There was an error while parsing parameters: {id}");

                return HandleParamValidationResponse(parameters, req);
            }

            Result<string> pathResponse = await _logPersistence.GetLogPathAsync(parameters.Id, ct);
            if (pathResponse.IsSuccess == false)
            {
                _logger.LogError($"There was an error retrieving path for log id: {id}");

                return HandleResult(pathResponse, req);
            }

            Result<string> logsResponse = await _payloadPersistence.GetPayloadAsync(pathResponse.Value, ct);

            return HandleResult(logsResponse, req);
        }
        catch (Exception ex)
        {
            _logger.LogError($"There was an error while executing function: {ex.Message}");

            response = req.CreateResponse(HttpStatusCode.InternalServerError);

            return response;
        }
    }

    private Parameters ValidateParameters(string id)
    {
        Parameters result = new Parameters();

        Guid idGuid;
        if (!Guid.TryParse(id, out idGuid))
        {
            result.ErrorMessage = $"Invalid guid {id}, should be in guid format like: {Guid.Empty}.";
            return result;
        }

        result.Id = idGuid;

        return result;
    }
}
