using System;
using System.IO;

namespace Arcadia
{
    /// <summary>
    /// A simplistic class to log info to a file
    /// </summary>
    public static class Log
    {
        /// <summary>
        /// The filename of the log
        /// </summary>
        private const string LogName = "info.log";

        /// <summary>
        /// Create a new log file
        /// </summary>
        public static void Initialize()
        {
            //Ensure that the log file contains only the latest application run log
            if (File.Exists(LogName))
                File.Delete(LogName);
            
            Write("Starting up Arcadia");
        }

        /// <summary>
        /// Write some information to the log
        /// </summary>
        /// <param name="Information">The information to append to the log</param>
        public static void Write(string Information)
        {
            File.AppendAllText(LogName, $"[LOG] {DateTime.Now.ToUniversalTime()}: {Information + Environment.NewLine}");
        }
    }
}
