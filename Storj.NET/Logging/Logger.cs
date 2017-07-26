using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StorjDotNet.Logging
{
    public class Logger
    {
        public Logger(LogLevel level = LogLevel.Warn)
        {
            LogLevel = level;
        }

        public LogLevel LogLevel { get; set; }

        public virtual void LogMessage(LogLevel level, string message)
        {
        }

        internal string GetLogLevelString(LogLevel level)
        {
            switch (level)
            {
                case LogLevel.Info:
                    return "Info";
                case LogLevel.Warn:
                    return "Warning";
                case LogLevel.Error:
                    return "Error:";
                case LogLevel.Debug:
                    return "Debug";
                default:
                    return string.Empty;
            }
        }
    }
}
