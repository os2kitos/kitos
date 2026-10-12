using System;
using System.Diagnostics;

namespace Digst.OioIdws.Common.Logging
{
    class TraceLogger : ILogger
    {
        /// <summary>
        /// The source to use for logging
        /// </summary>
        private static readonly TraceSource Source;

        static TraceLogger()
        {
            Source = new TraceSource("Digst.OioIdws");
        }

        public void WriteCore(TraceEventType eventType, int eventId, object state, Exception? exception, Func<object, Exception, string> formatter)
        {
            if (Source.Switch.ShouldTrace(eventType))
            {
                if(exception != null)
                {
                    Source.TraceEvent(eventType, eventId, formatter(state, exception));
                }
                else
                {
                    Source.TraceEvent(eventType, eventId, formatter(state, new Exception("Unhandled exception has occured")));
                }
            }
        }
    }
}
