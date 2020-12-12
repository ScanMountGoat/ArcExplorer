using Serilog.Core;
using Serilog.Events;
using System;

namespace ArcExplorer.Logging
{
    internal sealed class ApplicationSink : ILogEventSink
    {
        public static Lazy<ApplicationSink> Instance { get; } = new Lazy<ApplicationSink>(new ApplicationSink());

        /// <summary>
        /// The number of log events with <see cref="LogEventLevel.Error"/> that have occurred.
        /// </summary>
        public int ErrorCount { get; private set; } = 0;

        /// <summary>
        /// Occurs whenever a log event is handled regardless of severity.
        /// </summary>
        public event EventHandler? LogEventHandled;

        private ApplicationSink()
        {

        }

        public void Emit(LogEvent logEvent)
        {
            // Trigger an event whenever a log event occurs.
            // Just maintain a count for now.
            // TODO: Store the actual messages in memory to be displayed in the UI somehow.
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
                    ErrorCount++;
                    LogEventHandled?.Invoke(this, EventArgs.Empty);
                    break;
                case LogEventLevel.Fatal:
                    break;
                default:
                    break;
            }
        }
    }
}
