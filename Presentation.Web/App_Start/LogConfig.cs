using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;
using Serilog.Exceptions.Core;
using Serilog.Formatting.Compact;
using Serilog.Formatting.Elasticsearch;
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
                /*.WriteTo.Elasticsearch(new ElasticsearchSinkOptions
                {
                    AutoRegisterTemplate = true,
                    AutoRegisterTemplateVersion = AutoRegisterTemplateVersion.ESv7
                })*/
                .WriteTo.File(new CompactJsonFormatter(), path: @"C:\Logs\Kitos-.txt", retainedFileCountLimit: 10, rollingInterval: RollingInterval.Day)
                .CreateLogger();
        }

        public static void RegisterLog()
        {
            Log.Logger = GlobalLoggerInstance.Value;
        }
    }
}