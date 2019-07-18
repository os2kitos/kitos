using System;
using Serilog;
using Serilog.Events;
using Serilog.Exceptions.Destructurers;
using SerilogWeb.Classic;
using SerilogWeb.Classic.Enrichers;

namespace Presentation.Web
{
    public static class LogConfig
    {
        private static readonly Lazy<ILogger> GlobalLoggerInstance = new Lazy<ILogger>(ConfigureAndCreateSerilogLogger);

        public static ILogger GlobalLogger => GlobalLoggerInstance.Value;

        private static ILogger ConfigureAndCreateSerilogLogger()
        {
            ApplicationLifecycleModule.LogPostedFormData = LogPostedFormDataOption.Always;
            ApplicationLifecycleModule.RequestLoggingLevel = LogEventLevel.Debug;
            ApplicationLifecycleModule.LogRequestBody = true;
            ApplicationLifecycleModule.LogResponseBody = true;

            return new LoggerConfiguration()
                .ReadFrom.AppSettings()
                .Enrich.With<HttpRequestIdEnricher>()
                .Enrich.With<HttpSessionIdEnricher>()
                .Enrich.With<UserNameEnricher>()
                .Enrich.With<HttpRequestUserAgentEnricher>()
                .Enrich.With<ExceptionEnricher>()
                .Enrich.With<HttpRequestClientHostIPEnricher>()
                //.WriteTo.Trace()
                .CreateLogger();
        }

        public static void RegisterLog()
        {
            Log.Logger = GlobalLoggerInstance.Value;
        }
    }
}