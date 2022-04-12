using System;
using Serilog;
using Serilog.Exceptions.Core;
using Serilog.Formatting.Compact;
using Serilog.Formatting.Json;
using Serilog.Sinks.Elasticsearch;
using Serilog.Sinks.File;

namespace Presentation.Web
{
    public static class LogConfig
    {
        private static readonly Lazy<ILogger> GlobalLoggerInstance = new Lazy<ILogger>(ConfigureAndCreateSerilogLogger);

        public static ILogger GlobalLogger => GlobalLoggerInstance.Value;

        private static ILogger ConfigureAndCreateSerilogLogger()
        {
            return new LoggerConfiguration()
                .ReadFrom.AppSettings()
                .Enrich.FromLogContext()
                .Enrich.With<ExceptionEnricher>()
                .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(
                    new Uri("http://10.212.74.11:9200/"))
                {
                    AutoRegisterTemplate = true,
                    IndexFormat = "test-index-{0:yyyy.MM.dd}",
                    DeadLetterIndexName = "test-deadletter-{0:yyyy.MM.dd}",
                    FailureCallback = e => Console.WriteLine("Unable to submit event " + e.MessageTemplate),
                    EmitEventFailure = EmitEventFailureHandling.WriteToSelfLog |
                                       EmitEventFailureHandling.WriteToFailureSink |
                                       EmitEventFailureHandling.RaiseCallback,
                    FailureSink = new FileSink(@"C:\Logs\Kitos-Failure-.txt", new JsonFormatter(), null)
                })
                .WriteTo.File(new CompactJsonFormatter(), path: @"C:\Logs\Kitos-.txt", retainedFileCountLimit: 10, rollingInterval: RollingInterval.Day)
                .CreateLogger();
        }

        public static void RegisterLog()
        {
            Log.Logger = GlobalLoggerInstance.Value;
        }
    }
}