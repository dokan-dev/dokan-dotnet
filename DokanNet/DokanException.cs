using System;

namespace DokanNet
{
    [Serializable]
    public class DokanException : Exception
    {
        /// <summary>
        /// Initialize the <see cref="DokanException"/> with a <see cref="Exception.HResult"/>
        /// </summary>
        /// <param name="status">The status for <see cref="Exception.HResult"/> </param>
        /// <param name="message">The error message</param>
        internal DokanException(int status, string message) : base(message)
        {
            HResult = status;
        }
    }
}