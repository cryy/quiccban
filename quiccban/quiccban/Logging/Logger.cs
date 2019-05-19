using Microsoft.Extensions.Logging;
using System;
using Console = Colorful.Console;
using System.Drawing;

namespace quiccban.Logging
{
    public class Logger : ILogger
    {
        private static object _lock = new object();
        private readonly string _name;
        
        public Logger(string name)
        {
            _name = name;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            lock (_lock)
            {

                var prefix = GetUniformPrefix();
                switch(logLevel)
                {
                    case LogLevel.None:
                        break;
                    case LogLevel.Information:
                        Console.WriteLineFormatted("{0}     [Info] {1}", Color.LightGray, Color.LightBlue, prefix, formatter(state, exception));
                        break;
                    case LogLevel.Debug:
                        Console.WriteLineFormatted("{0}    [Debug] {1}", Color.LightGray, Color.LightSeaGreen, prefix, formatter(state, exception));
                        break;
                    case LogLevel.Trace:
                        Console.WriteLineFormatted("{0}    [Trace] {1}", Color.LightGray, Color.Aqua, prefix, formatter(state, exception));
                        break;
                    case LogLevel.Warning:
                        Console.WriteLineFormatted("{0}     [Warn] {1}", Color.LightGray, Color.Yellow, prefix, formatter(state, exception));
                        break;
                    case LogLevel.Error:
                        Console.WriteLineFormatted("{0}    [Error] {1}", Color.LightGray, Color.IndianRed, prefix, formatter(state, exception));
                        break;
                    case LogLevel.Critical:
                        Console.WriteLineFormatted("{0} [Critical] {1}", Color.LightGray, Color.Red, prefix, formatter(state, exception));
                        break;

                }
            }
        }

        private string GetUniformPrefix()
            => $"   {DateTime.Now.ToString("hh:mm:ss")} ";
    }
}
