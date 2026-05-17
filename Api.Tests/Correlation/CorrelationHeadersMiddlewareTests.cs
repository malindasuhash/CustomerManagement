using Api.Correlation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using StateManagment.Models;

namespace Api.Tests.Correlation
{
    public class CorrelationHeadersMiddlewareTests
    {
        [Fact]
        public async Task Returns400_WhenHeadersMissing()
        {
            var builder = new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddSingleton<ICorrelationContextAccessor, CorrelationContextAccessor>();
                    services.AddLogging();
                })
                .Configure(app =>
                {
                    app.UseMiddleware<CorrelationHeadersMiddleware>();
                    app.Run(async ctx => { await ctx.Response.WriteAsync("ok"); });
                });

            using var server = new TestServer(builder);
            using var client = server.CreateClient();

            var res = await client.GetAsync("/");

            Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
        }

        [Fact]
        public async Task SetsContext_WhenHeadersPresent()
        {
            var builder = new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddSingleton<ICorrelationContextAccessor, CorrelationContextAccessor>();
                    services.AddLogging();
                })
                .Configure(app =>
                {
                    app.UseMiddleware<CorrelationHeadersMiddleware>();
                    app.Run(ctx =>
                    {
                        var accessor = ctx.RequestServices.GetRequiredService<ICorrelationContextAccessor>();
                        var ctxVal = accessor.Context;
                        if (ctxVal is null) ctx.Response.StatusCode = 500;
                        return ctx.Response.WriteAsync("ok");
                    });
                });

            using var server = new TestServer(builder);
            using var client = server.CreateClient();

            var id = Guid.NewGuid().ToString();
            client.DefaultRequestHeaders.Add(CorrelationHeadersMiddleware.CorrelationHeader, id);
            client.DefaultRequestHeaders.Add(CorrelationHeadersMiddleware.ClientHeader, "client-x");

            var res = await client.GetAsync("/");

            res.EnsureSuccessStatusCode();
        }
    }
}
