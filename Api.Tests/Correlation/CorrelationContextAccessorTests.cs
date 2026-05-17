using Api.Correlation;
using StateManagment.Models;
using System;
using Xunit;

namespace Api.Tests.Correlation
{
    public class CorrelationContextAccessorTests
    {
        [Fact]
        public async System.Threading.Tasks.Task ContextFlowsAcrossAsyncAwait()
        {
            var accessor = new CorrelationContextAccessor();

            var id = System.Guid.NewGuid();
            accessor.Context = new CorrelationContext(id, "client-1");

            // Flow across awaits
            await System.Threading.Tasks.Task.Yield();

            var ctx = accessor.Context;

            Assert.NotNull(ctx);
            Assert.Equal(id, ctx.CorrelationId);
            Assert.Equal("client-1", ctx.ClientId);
        }
    }
}
