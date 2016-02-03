using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DokanNet.Logging
{
    using System.Diagnostics;

    public class TraceLogger : ILogger
    {
        public void Debug(string message, params object[] args)
        {
            Trace.TraceInformation(message, args);
        }

        public void Info(string message, params object[] args)
        {
            Trace.TraceInformation(message, args);
        }

        public void Warn(string message, params object[] args)
        {
            Trace.TraceWarning(message, args);
        }

        public void Error(string message, params object[] args)
        {
            Trace.TraceError(message, args);
        }

        public void Fatal(string message, params object[] args)
        {
            Trace.TraceError(message, args);
        }
    }
}
