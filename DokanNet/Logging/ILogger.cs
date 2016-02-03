namespace DokanNet.Logging
{
    public interface ILogger
    {
        void Debug(string format, params object[] args);

        void Info(string format, params object[] args);

        void Warn(string format, params object[] args);

        void Error(string format, params object[] args);

        void Fatal(string format, params object[] args);
    }
}