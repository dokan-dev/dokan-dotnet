using System;

namespace DokanNet.Logging
{
    /// <summary>
    /// Handle log messages with callbacks
    /// </summary>
    public class Logger : ILogger
    {
        private readonly Action<string, object[]> _debug;
        private readonly Action<string, object[]> _info;
        private readonly Action<string, object[]> _warn;
        private readonly Action<string, object[]> _error;
        private readonly Action<string, object[]> _fatal;

        /// <summary>
        /// Initializes a new instance of the <see cref="Logger"/> class.
        /// </summary>
        /// <param name="debug">An <see cref="Action{T}"/> that are called when a debug log message are arriving.</param>
        /// <param name="info">An <see cref="Action{T}"/> that are called when a information log message are arriving</param>
        /// <param name="warn">An <see cref="Action{T}"/> that are called when a warning log message are arriving</param>
        /// <param name="error">An <see cref="Action{T}"/> that are called when a error log message are arriving</param>
        /// <param name="fatal">An <see cref="Action{T}"/> that are called when a fatal error log message are arriving</param>
        public Logger(
            Action<string, object[]> debug,
            Action<string, object[]> info,
            Action<string, object[]> warn,
            Action<string, object[]> error,
            Action<string, object[]> fatal)
        {
            _debug = debug;
            _info = info;
            _warn = warn;
            _error = error;
            _fatal = fatal;
        }

        /// <inheritdoc />
        public void Debug(string message, params object[] args)
        {
            _debug(message, args);
        }

        /// <inheritdoc />
        public void Info(string message, params object[] args)
        {
            _info(message, args);
        }

        /// <inheritdoc />
        public void Warn(string message, params object[] args)
        {
            _warn(message, args);
        }

        /// <inheritdoc />
        public void Error(string message, params object[] args)
        {
            _error(message, args);
        }

        /// <inheritdoc />
        public void Fatal(string message, params object[] args)
        {
            _fatal(message, args);
        }
    }
}