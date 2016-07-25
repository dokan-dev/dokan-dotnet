using System;

namespace DokanNet
{
    [Serializable]
    public class DokanException : Exception
    {
        internal DokanException(int status, string message) : base(message)
        {
            HResult = status;
        }
    }
}