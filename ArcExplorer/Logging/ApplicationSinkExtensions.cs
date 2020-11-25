using Serilog;
using Serilog.Configuration;

namespace ArcExplorer.Logging
{
    internal static class ApplicationSinkExtensions
    {
        public static LoggerConfiguration ApplicationSink(this LoggerSinkConfiguration loggerConfiguration)
        {
            return loggerConfiguration.Sink(new ApplicationSink());
        }
    }
}
