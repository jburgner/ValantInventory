using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ValantInventoryExerciseCore.Tests
{

    //mock an ILogger to capture logging messages for unit testing

    public class TestLogger : ILogger
    {

        public string Message { get; set; }

        public TestLogger()
        {
            Message = "";
        }

        public void Log<TState>(LogLevel level, EventId eventId, TState state, Exception ex, Func<TState, Exception, String> formatter)
        {

            Message += state.ToString();

            return;
        }

        public bool IsEnabled(LogLevel level)
        {
            return true;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return (IDisposable)state;
        }
    }

    public class TestLoggerFactory: ILoggerFactory
    {

        public TestLogger Logger { get; set; }

        public ILogger CreateLogger(string className)
        {
            Logger = new TestLogger();
            return Logger;
        }

        public ILogger CreateLogger<T>()
        {
            Logger = new TestLogger();
            return Logger;
        }

        public void Dispose()
        {
            
        }

        public void AddProvider(ILoggerProvider provider)
        {

        }

    }
}
