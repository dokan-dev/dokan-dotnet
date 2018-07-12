using System;

namespace DokanNet
{
    /// <summary>
    /// The dokan exception.
    /// </summary>
    [Serializable]
    public class DokanException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DokanException"/> class with a <see cref="Exception.HResult"/>.
        /// </summary>
        /// <param name="status">
        /// The error status also written to <see cref="Exception.HResult"/>.
        /// </param>
        /// <param name="message">
        /// The error message.
        /// </param>
        internal DokanException(DokanStatus status, string message)
            : base(message)
        {
            ErrorStatus = status;
            HResult = (int)status;
        }

        public DokanStatus ErrorStatus { get; private set; }
    }
}