using System;
using System.Diagnostics;
using Caliburn.Micro;
using System.Linq;
using System.Collections.Generic;

namespace EasyPlayer
{
    public static class Logger
    {
        static Logger()
        {
            LogManager.GetLog = type => new DefaultLogger(type);
            Logger.NamespaceIncludeFilters = new[] { "EasyPlayer" };
        }

        private static LinkedList<string> messages = new LinkedList<string>();

        public static ILog Log<T>()
        {
            return LogManager.GetLog(typeof(T));
        }

        /// <summary>
        /// If populated, the namespaces to log, otherwise, everything will be logged.
        /// </summary>
        public static string[] NamespaceIncludeFilters { get; set; }
        public static IEnumerable<string> RecentLogMessages { get { return messages; } }

        private static void Write(string message)
        {
            messages.AddLast(message);
            while (messages.Count > 10000) messages.RemoveFirst();
        }

        private class DefaultLogger : ILog
        {
            private readonly Type type;

            public DefaultLogger(Type type)
            {
                this.type = type;
            }

            public void Error(Exception exception)
            {
                Write("Error", "{0}", exception);
            }

            public void Info(string format, params object[] args)
            {
                Write("Info", format, args);
            }

            public void Warn(string format, params object[] args)
            {
                Write("Warn", format, args);
            }

            private void Write(string level, string format, params object[] args)
            {
                if (Logger.NamespaceIncludeFilters != null)
                {
                    if (!Logger.NamespaceIncludeFilters.Any(ns => type.Namespace.StartsWith(ns))) return;
                }

                var message = string.Format("{0} {1} [{2}] {3}", DateTime.Now.ToString("HH:mm:ss.fff"), level, type.Name, string.Format(format, args));
                Debug.WriteLine(message);
                Logger.Write(message);
            }
        }
    }
}
