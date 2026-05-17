using StateManagment.Models;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Api.Correlation
{
    public class CorrelationPropagationHandler : DelegatingHandler
    {
        private readonly ICorrelationContextAccessor _accessor;

        public CorrelationPropagationHandler(ICorrelationContextAccessor accessor)
        {
            _accessor = accessor;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var ctx = _accessor.Context;
            if (ctx is not null)
            {
                if (!request.Headers.Contains(CorrelationHeadersMiddleware.CorrelationHeader))
                    request.Headers.Add(CorrelationHeadersMiddleware.CorrelationHeader, ctx.CorrelationId.ToString());

                if (!request.Headers.Contains(CorrelationHeadersMiddleware.ClientHeader))
                    request.Headers.Add(CorrelationHeadersMiddleware.ClientHeader, ctx.ClientId);
            }

            return base.SendAsync(request, cancellationToken);
        }
    }
}
