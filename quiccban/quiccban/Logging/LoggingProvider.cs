using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace quiccban.Logging
{
    public class LoggingProvider : ILoggerProvider
    {
        private readonly ConcurrentDictionary<string, Logger> _loggers = new ConcurrentDictionary<string, Logger>();

        public ILogger CreateLogger(string categoryName)
        {
            return _loggers.GetOrAdd(categoryName, name => new Logger(name));
        }


        public void Dispose()
        {
            _loggers.Clear();
        }
    }
}
