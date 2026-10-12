using Microsoft.AspNetCore.Builder;
using Serilog;
using System;
using System.IO;

namespace Presentation.Web.Infrastructure.Configuration
{
    internal static class SerilogConfigExtensions
    {
        internal static void AddKitosSerilog(this WebApplicationBuilder builder)
        {
            var configuredLogPath = builder.Configuration["AppSettings:LogFilePath"];
            var logPath = string.IsNullOrWhiteSpace(configuredLogPath)
                ? Path.Combine(Path.GetTempPath(), "kitos", "Kitos-.txt")
                : configuredLogPath;
            builder.Configuration["Serilog:WriteTo:0:Args:path"] = logPath;

            var logDirectory = Path.GetDirectoryName(logPath);
            if (!string.IsNullOrEmpty(logDirectory))
            {
                try
                {
                    Directory.CreateDirectory(logDirectory);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Unable to create log directory '{logDirectory}'.", ex);
                }
            }

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(builder.Configuration)
                .CreateLogger();

            builder.Host.UseSerilog();
        }
    }
}
