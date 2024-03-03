using Fetcher.DTOs;
using Microsoft.Azure.Functions.Worker.Http;
using System.Net;

namespace Fetcher.Functions;

public class FunctionBase
{
    protected record ParametersBase
    {
        public string? ErrorMessage { get; set; }
    }

    protected HttpResponseData HandleResult(Result<string> result, HttpRequestData req)
    {
        HttpResponseData response;

        if (result is null)
        {
            response = req.CreateResponse(HttpStatusCode.NotFound);
            return response;
        }

        if (result.IsSuccess && result.Value is not null)
        {
            response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "text/plain; charset=utf-8");
            response.WriteString(result.Value ?? string.Empty);
        }

        if (result.IsSuccess && result.Value is null)
        {
            response = req.CreateResponse(HttpStatusCode.NotFound);
            return response;
        }

        response = req.CreateResponse(HttpStatusCode.InternalServerError);
        return response;
    }

    protected HttpResponseData HandleParamValidationResponse(ParametersBase parameters, HttpRequestData req)
    {
        HttpResponseData response;

        response = req.CreateResponse(HttpStatusCode.BadRequest);
        response.WriteString(parameters.ErrorMessage!);

        return response;
    }
}
