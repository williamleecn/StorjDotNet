using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StorjDotNet.Logging
{
    public class ConsoleLogger : Logger
    {
        public ConsoleLogger(LogLevel level = LogLevel.Warn) : base(level)
        {
            
        }

        public override void LogMessage(LogLevel level, string message)
        {
            if (level >= LogLevel)
                Console.WriteLine($"{GetLogLevelString(level)}: {message}");
        }
    }
}
