using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using StateManagment.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Api.Correlation
{
    public class CorrelationHeadersMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<CorrelationHeadersMiddleware> _logger;

        public const string CorrelationHeader = "nwp-correlation-id";
        public const string ClientHeader = "nwp-client-id";

        public CorrelationHeadersMiddleware(RequestDelegate next, ILogger<CorrelationHeadersMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, ICorrelationContextAccessor accessor)
        {
            if (!context.Request.Headers.TryGetValue(CorrelationHeader, out var correlationValues) || string.IsNullOrWhiteSpace(correlationValues))
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;

                var errorCorrelationId = CreateRfcClientError($"Missing required header: {CorrelationHeader}", "#missing-header", new Dictionary<string, object>
                    {
                        { "missingHeader", CorrelationHeader }
                    });

                context.Response.ContentType = "application/problem+json";
                await context.Response.WriteAsJsonAsync(errorCorrelationId);
                return;
            }

            if (!context.Request.Headers.TryGetValue(ClientHeader, out var clientValues) || string.IsNullOrWhiteSpace(clientValues))
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                var errorClientId = CreateRfcClientError($"Missing required header: {ClientHeader}", "#missing-header", new Dictionary<string, object>
                    {
                        { "missingHeader", ClientHeader }
                    });

                context.Response.ContentType = "application/problem+json";
                await context.Response.WriteAsJsonAsync(errorClientId);
                return;
            }

            if (!Guid.TryParse(correlationValues.ToString(), out var correlationId))
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                var errorGuid = CreateRfcClientError($"Formatting error: {ClientHeader}", "#header-format", new Dictionary<string, object>
                    {
                        { "incorrect-format", "GUID required" }
                    });

                context.Response.ContentType = "application/problem+json";
                await context.Response.WriteAsJsonAsync(errorGuid);
            }

            var clientId = clientValues.ToString();

            var correlation = new CorrelationContext(correlationId, clientId);
            accessor.Context = correlation;

            using (_logger.BeginScope(new Dictionary<string, object>
            {
                [CorrelationHeader] = correlationId,
                [ClientHeader] = clientId
            }))
            {
                await _next(context);
            }
        }

        private static ApiContract.Rfc7807 CreateRfcClientError(string message, string fragment, Dictionary<string, object> parameters)
        {
            var rfcError = new ApiContract.Rfc7807()
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Bad Request",
                Detail = message,
                Type = fragment,
                Parameters = parameters
            };

            return rfcError;
        }
    }
}
