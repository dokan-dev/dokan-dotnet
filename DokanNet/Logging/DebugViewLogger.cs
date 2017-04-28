using System.Runtime.InteropServices;

namespace DokanNet.Logging
{
    /// <summary>
    /// Write log using OutputDebugString 
    /// </summary>
    /// <remarks>
    /// To see the output in visual studio 
    /// Project + %Properties, Debug tab, check "Enable unmanaged code debugging".
    /// </remarks> 
    public class DebugViewLogger : ILogger
    {
        private readonly string _loggerName;

        /// <summary>
        /// Initializes a new instance of the <see cref="DebugViewLogger"/> class.
        /// </summary>
        /// <param name="loggerName">Optional name to be added to each log line.</param>
        public DebugViewLogger(string loggerName = "")
        {
            _loggerName = loggerName;
        }

        /// <inheritdoc />
        public void Debug(string message, params object[] args)
        {
            WriteMessageToDebugView("debug", message, args);
        }

        /// <inheritdoc />
        public void Info(string message, params object[] args)
        {
            WriteMessageToDebugView("info", message, args);
        }

        /// <inheritdoc />
        public void Warn(string message, params object[] args)
        {
            WriteMessageToDebugView("warn", message, args);
        }

        /// <inheritdoc />
        public void Error(string message, params object[] args)
        {
            WriteMessageToDebugView("error", message, args);
        }

        /// <inheritdoc />
        public void Fatal(string message, params object[] args)
        {
            WriteMessageToDebugView("fatal", message, args);
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        private static extern void OutputDebugString(string message);

        private void WriteMessageToDebugView(string category, string message, params object[] args)
        {
            if (args?.Length > 0)
                message = string.Format(message, args);
            OutputDebugString(message.FormatMessageForLogging(category, _loggerName));
        }
    }
}