namespace DokanNet.Logging
{
    /// <summary>
    /// Ignore all log messages.
    /// </summary>
    public class NullLogger : ILogger
    {
        /// <inheritdoc />
        public void Debug(string message, params object[] args)
        {
        }

        /// <inheritdoc />
        public void Error(string message, params object[] args)
        {
        }

        /// <inheritdoc />
        public void Fatal(string message, params object[] args)
        {
        }

        /// <inheritdoc />
        public void Info(string message, params object[] args)
        {
        }

        /// <inheritdoc />
        public void Warn(string message, params object[] args)
        {
        }
    }
}