using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Presentation.Web.Infrastructure.Configuration;
using Serilog;
using Serilog.Core;
using Serilog.AspNetCore;
using Serilog.Events;
using Xunit;

namespace Tests.Unit.Presentation.Web.Infrastructure
{
    public class RequestCancellationLoggingTest
    {
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task RequestLogging_OnlySuppressesCancellationForAbortedRequests(bool requestAborted)
        {
            var sink = new EventSink();
            using var logger = new LoggerConfiguration().WriteTo.Sink(sink).CreateLogger();
            using var services = new ServiceCollection()
                .AddLogging()
                .AddSerilog(logger)
                .Configure<RequestLoggingOptions>(options => options.Logger = logger)
                .BuildServiceProvider();
            var app = new ApplicationBuilder(services);
            app.UseKitosRequestLogging();
            app.Run(_ => throw new OperationCanceledException());
            var context = new DefaultHttpContext
            {
                RequestServices = services,
                RequestAborted = new CancellationToken(requestAborted)
            };

            var exception = await Record.ExceptionAsync(() => app.Build()(context));

            Assert.True(sink.Events.Count > 0, exception?.ToString() ?? "No exception and no log events");
            var logEvent = Assert.Single(sink.Events);
            if (requestAborted)
            {
                Assert.Null(exception);
                Assert.Null(logEvent.Exception);
                Assert.Equal(LogEventLevel.Information, logEvent.Level);
                Assert.Equal(StatusCodes.Status499ClientClosedRequest, context.Response.StatusCode);
            }
            else
            {
                Assert.IsType<OperationCanceledException>(exception);
                Assert.Equal(LogEventLevel.Error, logEvent.Level);
            }
        }

        private sealed class EventSink : ILogEventSink
        {
            public List<LogEvent> Events { get; } = new();
            public void Emit(LogEvent logEvent) => Events.Add(logEvent);
        }
    }
}
