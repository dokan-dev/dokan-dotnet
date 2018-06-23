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
        /// The status for <see cref="Exception.HResult"/>.
        /// </param>
        /// <param name="message">
        /// The error message.
        /// </param>
        internal DokanException(int status, string message) : base(String.Format ("{0}. (Dokan hr=0X{1:X})", message, status))
        {
            HResult = status;
        }
    }
}