using Serilog.Core;
using Serilog.Events;
using System;

namespace ArcExplorer.Logging
{
    internal class ApplicationSink : ILogEventSink
    {
        public void Emit(LogEvent logEvent)
        {
            // TODO: Update the GUI based on the log level.
            switch (logEvent.Level)
            {
                case LogEventLevel.Verbose:
                    break;
                case LogEventLevel.Debug:
                    break;
                case LogEventLevel.Information:
                    break;
                case LogEventLevel.Warning:
                    break;
                case LogEventLevel.Error:
                    break;
                case LogEventLevel.Fatal:
                    break;
                default:
                    break;
            }
        }
    }
}
