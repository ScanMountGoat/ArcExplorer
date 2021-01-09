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
        /// Occurs whenever a log event with severity <see cref="LogEventLevel.Error"/> is generated.
        /// </summary>
        public event EventHandler? ErrorEventRaised;

        private ApplicationSink()
        {

        }

        public void Emit(LogEvent logEvent)
        {
            switch (logEvent.Level)
            {
                case LogEventLevel.Error:
                    ErrorCount++;
                    LogMessages.Add(logEvent.RenderMessage());
                    ErrorEventRaised?.Invoke(this, EventArgs.Empty);
                    break;
                default:
                    break;
            }
        }
    }
}
