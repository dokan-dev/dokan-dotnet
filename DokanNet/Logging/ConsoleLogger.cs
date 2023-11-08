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
        private bool _disposed;

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
        public bool DebugEnabled => true;

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
            lock (Console.Out) // we only need this lock because we want to have more than one logger in this version
            {
                var origForegroundColor = Console.ForegroundColor;
                Console.ForegroundColor = newColor;
                Console.WriteLine(message.FormatMessageForLogging(true, loggerName: _loggerName));
                Console.ForegroundColor = origForegroundColor;
            }
        }

        /// <summary>
        /// Dispose the object from the native resources.
        /// </summary>
        /// <param name="disposing">Whether it was call by <see cref="Dispose()"/></param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // dispose managed state (managed objects)
                    _PendingLogs.CompleteAdding();
                    _WriterTask.Wait();
                }

                // free unmanaged resources (unmanaged objects) and override finalizer
                // set large fields to null
                _disposed = true;
            }
        }

        /// <summary>
        /// Dispose the object from the native resources.
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}