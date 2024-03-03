using Fetcher.DTOs;
using Fetcher.Persistence;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;

namespace Fetcher.Functions;

public sealed class GetLogsFunction : FunctionBase
{
    protected record Parameters : ParametersBase
    {
        public DateTime From { get; set; }
        public DateTime To { get; set; }
    }

    private readonly ILogPersistence _logPersistence;
    private readonly ILogger<FetchDataFunction> _logger;

    public GetLogsFunction(ILogPersistence logPersistence, ILogger<FetchDataFunction> logger)
    {
        _logPersistence = logPersistence;
        _logger = logger;
    }

    [Function("GetLogsFunction")]
    public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "logs/from/{from}/to/{to}")] HttpRequestData req, string from, string to, CancellationToken ct)
    {
        HttpResponseData response;

        _logger.LogInformation("C# HTTP trigger function processed a request.");

        try
        {
            Parameters parameters = ValidateParameters(from, to);
            if (parameters.ErrorMessage != null)
            {
                _logger.LogError($"There was an error while parsing parameters: {from}, {to}");

                return HandleParamValidationResponse(parameters, req);
            }

            Result<string> logsResponse = await _logPersistence.GetLogsAsync(parameters.From, parameters.To, ct);

            return HandleResult(logsResponse, req);
        }
        catch (Exception ex)
        {
            _logger.LogError($"There was an error while executing function: {ex.Message}");

            response = req.CreateResponse(HttpStatusCode.InternalServerError);

            return response;
        }
    }

    private Parameters ValidateParameters(string from, string to)
    {
        Parameters result = new Parameters();

        DateTime fromDate;
        if (!DateTime.TryParse(from, out fromDate))
        {
            result.ErrorMessage = $"Invalid date {from}, should be in format: yyyy-MM-dd, eg. 2024-02-28.";
            return result;
        }

        DateTime toDate;
        if (!DateTime.TryParse(to, out toDate))
        {
            result.ErrorMessage = $"Invalid date {to}, should be in format: yyyy-MM-dd, eg. 2024-02-28.";
            return result;
        }

        if (toDate <= fromDate)
        {
            result.ErrorMessage = $"Date 'to' must be less then 'from'.";
            return result;
        }

        result.From = fromDate;
        result.To = toDate;

        return result;
    }
}
