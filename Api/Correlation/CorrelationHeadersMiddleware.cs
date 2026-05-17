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
                await context.Response.WriteAsync($"Missing required header: {CorrelationHeader}");
                return;
            }

            if (!context.Request.Headers.TryGetValue(ClientHeader, out var clientValues) || string.IsNullOrWhiteSpace(clientValues))
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsync($"Missing required header: {ClientHeader}");
                return;
            }

            if (!Guid.TryParse(correlationValues.ToString(), out var correlationId))
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsync($"Invalid GUID in header: {CorrelationHeader}");
                return;
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
    }
}
