using Serilog;

namespace Presentation.Web.App_Start
{
    public static class LogConfig
    {
        public static void RegisterLog()
        {
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.AppSettings()
                .Destructure.ByTransforming<Core.DomainModel.User>(u => new {u.Id, u.Name, u.LastName, u.Uuid})
                .WriteTo.Trace()
                .CreateLogger();
        }
    }
}