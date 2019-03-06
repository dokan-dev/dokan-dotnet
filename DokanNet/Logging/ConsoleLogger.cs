using System;
using System.Threading.Tasks;

namespace DokanNet.Logging
{
    /// <summary>
    /// Log to the console.
    /// </summary>
    public class ConsoleLogger : ILogger, IDisposable
    {
        private readonly string _loggerName;
        private readonly System.Collections.Concurrent.BlockingCollection<Tuple<String, ConsoleColor>> _PendingLogs
            = new System.Collections.Concurrent.BlockingCollection<Tuple<String, ConsoleColor>>();

        private readonly Task _WriterTask = null;
        /// <summary>
        /// Initializes a new instance of the <see cref="ConsoleLogger"/> class.
        /// </summary>
        /// <param name="loggerName">Optional name to be added to each log line.</param>
        public ConsoleLogger(string loggerName = "")
        {
            _loggerName = loggerName;
            _WriterTask = Task.Factory.StartNew(() =>
            {
                foreach (var tuple in _PendingLogs.GetConsumingEnumerable())
                {
                    WriteMessage(tuple.Item1, tuple.Item2);
                }
            });
        }

        /// <inheritdoc />
        public void Debug(string message, params object[] args)
        {
            EnqueueMessage(Console.ForegroundColor, message, args);
        }

        /// <inheritdoc />
        public void Info(string message, params object[] args)
        {
            EnqueueMessage(Console.ForegroundColor, message, args);
        }

        /// <inheritdoc />
        public void Warn(string message, params object[] args)
        {
            EnqueueMessage(ConsoleColor.DarkYellow, message, args);
        }

        /// <inheritdoc />
        public void Error(string message, params object[] args)
        {
            EnqueueMessage(ConsoleColor.Red, message, args);
        }

        /// <inheritdoc />
        public void Fatal(string message, params object[] args)
        {
            EnqueueMessage(ConsoleColor.Red, message, args);
        }

        private void EnqueueMessage(ConsoleColor newColor, string message, params object[] args)
        {
            if (args.Length > 0)
                message = string.Format(message, args);

            _PendingLogs.Add(Tuple.Create(message, newColor));
        }

        private void WriteMessage(string message, ConsoleColor newColor)
        {
            var origForegroundColor = Console.ForegroundColor;
            Console.ForegroundColor = newColor;
            Console.WriteLine(message.FormatMessageForLogging(true, loggerName: _loggerName));
            Console.ForegroundColor = origForegroundColor;
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    _PendingLogs.CompleteAdding();
                    _WriterTask.Wait();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~ConsoleLogger()
        // {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}