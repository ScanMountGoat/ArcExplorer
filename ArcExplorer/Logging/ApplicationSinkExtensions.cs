using Serilog;
using Serilog.Configuration;

namespace ArcExplorer.Logging
{
    internal static class ApplicationSinkExtensions
    {
        public static LoggerConfiguration ApplicationLog(this LoggerSinkConfiguration loggerConfiguration)
        {
            return loggerConfiguration.Sink(ApplicationSink.Instance.Value);
        }
    }
}
