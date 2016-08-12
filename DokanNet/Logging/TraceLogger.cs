using System.Diagnostics;

namespace DokanNet.Logging
{
    /// <summary>
    /// Write all log messages to the <see cref="Trace"/>.
    /// </summary>
    public class TraceLogger : ILogger
    {
        /// <inheritdoc />
        public void Debug(string message, params object[] args)
        {
            Trace.TraceInformation(message, args);
        }

        /// <inheritdoc />
        public void Info(string message, params object[] args)
        {
            Trace.TraceInformation(message, args);
        }

        /// <inheritdoc />
        public void Warn(string message, params object[] args)
        {
            Trace.TraceWarning(message, args);
        }

        /// <inheritdoc />
        public void Error(string message, params object[] args)
        {
            Trace.TraceError(message, args);
        }

        /// <inheritdoc />
        public void Fatal(string message, params object[] args)
        {
            Trace.TraceError(message, args);
        }
    }
}