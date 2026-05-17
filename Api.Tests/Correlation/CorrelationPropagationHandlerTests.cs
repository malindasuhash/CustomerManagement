using Api.Correlation;
using StateManagment.Models;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Api.Tests.Correlation
{
    public class CorrelationPropagationHandlerTests
    {
        [Fact]
        public async Task OutgoingRequestContainsHeaders_WhenContextPresent()
        {
            var accessor = new CorrelationContextAccessor();
            var id = Guid.NewGuid();
            accessor.Context = new CorrelationContext(id, "client-1");

            // Create inner handler that asserts headers
            var inner = new TestHandler(req =>
            {
                Assert.True(req.Headers.Contains(CorrelationHeadersMiddleware.CorrelationHeader));
                Assert.True(req.Headers.Contains(CorrelationHeadersMiddleware.ClientHeader));
                return new HttpResponseMessage(HttpStatusCode.OK);
            });

            var handler = new CorrelationPropagationHandler(accessor)
            {
                InnerHandler = inner
            };

            var client = new HttpClient(handler);

            var res = await client.GetAsync("http://example.local/");

            Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        }

        private class TestHandler : HttpMessageHandler
        {
            private readonly Func<HttpRequestMessage, HttpResponseMessage> _onSend;
            public TestHandler(Func<HttpRequestMessage, HttpResponseMessage> onSend) => _onSend = onSend;
            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
            {
                return Task.FromResult(_onSend(request));
            }
        }
    }
}
