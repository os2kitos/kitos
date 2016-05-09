using Serilog;

namespace Presentation.Web.App_Start
{
    public static class LogConfig
    {
        public static void RegisterLog()
        {
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.AppSettings()
                .WriteTo.Trace()
                .CreateLogger();
        }
    }
}