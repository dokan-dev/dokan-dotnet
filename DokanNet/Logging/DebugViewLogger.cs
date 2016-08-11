using System.Runtime.InteropServices;

namespace DokanNet.Logging
{
    /// <summary>
    /// To see the output in visual studio 
    /// Project + Properties, Debug tab, check "Enable unmanaged code debugging".
    /// </summary>
    public class DebugViewLogger : ILogger
    {
        private string loggerName;

        public DebugViewLogger(string loggerName = "")
        {
            this.loggerName = loggerName;
        }

        public void Debug(string message, params object[] args)
        {
            WriteMessageToDebugView("debug", message, args);
        }

        public void Info(string message, params object[] args)
        {
            WriteMessageToDebugView("info", message, args);
        }

        public void Warn(string message, params object[] args)
        {
            WriteMessageToDebugView("warn", message, args);
        }

        public void Error(string message, params object[] args)
        {
            WriteMessageToDebugView("error", message, args);
        }

        public void Fatal(string message, params object[] args)
        {
            WriteMessageToDebugView("fatal", message, args);
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private static extern void OutputDebugString(string message);

        private void WriteMessageToDebugView(string category, string message, params object[] args)
        {
            if (args?.Length > 0)
                message = string.Format(message, args);
            OutputDebugString(message.FormatMessageForLogging(category, loggerName));
        }
    }
}