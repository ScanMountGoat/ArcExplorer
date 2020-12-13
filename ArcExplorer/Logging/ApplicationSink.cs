using Serilog.Core;
using Serilog.Events;
using System;
using System.Collections.Generic;

namespace ArcExplorer.Logging
{
    internal sealed class ApplicationSink : ILogEventSink
    {
        public static Lazy<ApplicationSink> Instance { get; } = new Lazy<ApplicationSink>(new ApplicationSink());

        /// <summary>
        /// The number of log events with <see cref="LogEventLevel.Error"/> that have occurred.
        /// </summary>
        public int ErrorCount { get; private set; } = 0;

        public List<string> LogMessages { get; } = new List<string>();

        /// <summary>
        /// Occurs whenever a log event is handled regardless of severity.
        /// </summary>
        public event EventHandler? LogEventHandled;

        private ApplicationSink()
        {

        }

        public void Emit(LogEvent logEvent)
        {
            // TODO: Handle other log levels.
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
                    break;
                case LogEventLevel.Fatal:
                    break;
                default:
                    break;
            }

            // Trigger an event whenever a log event occurs.
            LogMessages.Add(logEvent.RenderMessage());
            LogEventHandled?.Invoke(this, EventArgs.Empty);
        }
    }
}
