using System;

namespace AssetCatalogue
{
    public class Logger
    {
        public delegate void LogMessage(string msg);
        public event LogMessage OnLogMessage;

        public void WriteLine(object obj) => WriteLine(obj.ToString());
        public void WriteLine(string line)
        {
            OnLogMessage?.Invoke(line);
            Console.WriteLine(line);
        }
    }
}