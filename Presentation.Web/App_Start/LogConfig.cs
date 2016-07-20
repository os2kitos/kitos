using Serilog;
using Serilog.Exceptions.Destructurers;
using SerilogWeb.Classic.Enrichers;

namespace Presentation.Web.App_Start
{
    public static class LogConfig
    {
        public static void RegisterLog()
        {
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.AppSettings()
                .Enrich.With<HttpRequestIdEnricher>()
                .Enrich.With<HttpSessionIdEnricher>()
                .Enrich.With<UserNameEnricher>()
                .Enrich.With<HttpRequestUserAgentEnricher>()
                .Enrich.With<ExceptionEnricher>()
                .Enrich.With<HttpRequestClientHostIPEnricher>()
                .WriteTo.Trace()
                .CreateLogger();
        }
    }
}