using System;

namespace DokanNet.Logging
{
    public class Logger : ILogger
    {
        private readonly Action<string, object[]> _debug;
        private readonly Action<string, object[]> _info;
        private readonly Action<string, object[]> _warn;
        private readonly Action<string, object[]> _error;
        private readonly Action<string, object[]> _fatal;

        public Logger(Action<string, object[]> debug, Action<string, object[]> info, Action<string, object[]> warn,
            Action<string, object[]> error, Action<string, object[]> fatal)
        {
            _debug = debug;
            _info = info;
            _warn = warn;
            _error = error;
            _fatal = fatal;
        }

        public void Debug(string message, params object[] args)
        {
            _debug(message, args);
        }

        public void Info(string message, params object[] args)
        {
            _info(message, args);
        }

        public void Warn(string message, params object[] args)
        {
            _warn(message, args);
        }

        public void Error(string message, params object[] args)
        {
            _error(message, args);
        }

        public void Fatal(string message, params object[] args)
        {
            _fatal(message, args);
        }
    }
}