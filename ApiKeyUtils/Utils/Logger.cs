namespace ApiKeyUtils.Utils
{
    /// <summary>
    /// Simple logger class for logging messages and exceptions to a log file. The log file is created in the 
    /// system's temporary directory with a timestamped name by default, but the path can be customized. The 
    /// logger provides methods for logging messages with different severity levels (Info, Warning, Error, 
    /// Critical) and for logging exceptions with optional stack traces.
    /// </summary>
    public static class Logger
    {
        private static string _autoLogName = $"ApiKeyUtils_{DateTime.Now:yyyyMMdd_HHmmss}.log";

        /// <summary>
        /// Log file full path. By default, it is set to a file named "ApiKeyUtils_{timestamp}.log" in the 
        /// system's temporary directory. You can change this path to specify a different location or filename 
        /// for the log file.
        /// </summary>
        public static string LogPath { get; set; } = Path.Combine(Path.GetTempPath(), _autoLogName);

        /// <summary>
        /// Deletes the log file if it exists.
        /// </summary>
        /// <returns>
        /// <see langword="True"/> if log file deleted or does not exist, or <see langword="false"/> if deletion fails.
        /// </returns>
        public static bool DeleteLogFile()
        {
            try
            {
                if (File.Exists(LogPath))
                    File.Delete(LogPath);                  

                return true;
            }
            catch
            {
                // If deletion fails then return false.
                return false;
            }           
        }

        /// <summary>
        /// Reads the contents of the log file and returns it as a string.
        /// </summary>
        /// <returns>
        /// Contents of the log file as a string (empty if log file does not exist or if there was a read error).
        /// </returns>
        public static string ReadLogFile()
        {
            try
            {
                if (File.Exists(LogPath))
                    return File.ReadAllText(LogPath);
                else
                    return string.Empty;
            }
            catch
            {
                // If reading fails, return empty string.
                return string.Empty;
            }
        }

        /// <summary>
        /// Logs a message to the log file with the specified severity level. Each log entry includes a timestamp.
        /// </summary>
        /// <param name="message">Message to log.</param>
        /// <param name="severity">Log severity (<see cref="LogSeverity"/>).</param>
        internal static void Log(string message, LogSeverity severity)
        {
            try
            {
                var logMessage = $"{DateTime.Now:dd-MM-yyyy HH:mm:ss} - {severity}: {message}{Environment.NewLine}";
                File.AppendAllText(LogPath, logMessage);
            }
            catch
            {
                // If logging fails, we silently ignore it to avoid impacting the main functionality.
            }
        }

        /// <summary>
        /// Logs an exception to the log file. The log entry includes the exception message and, optionally, the stack trace.
        /// </summary>
        /// <param name="ex">Exception to log.</param>
        /// <param name="includeStackTrace">Optionally include stack trace (default is <see langword="false"/>).</param>
        internal static void LogException(Exception ex, bool includeStackTrace = false)
        {
            Log(ex.Message, LogSeverity.Error);

            if (includeStackTrace && ex.StackTrace != null)
                File.AppendAllText(LogPath, $"Stack Trace: {ex.StackTrace}{Environment.NewLine}");
        }

        /// <summary>
        /// Logs an info message to the log file.
        /// </summary>
        /// <param name="message">Message to log.</param>
        internal static void LogInfo(string message) => Log(message, LogSeverity.Info);

        /// <summary>
        /// Logs a warning message to the log file.
        /// </summary>
        /// <param name="message">Message to log.</param>
        internal static void LogWarning(string message) => Log(message, LogSeverity.Warning);

        /// <summary>
        /// Logs an error message to the log file.
        /// </summary>
        /// <param name="message">Message to log.</param>
        internal static void LogError(string message) => Log(message, LogSeverity.Error);

        /// <summary>
        /// Logs a critical message to the log file.
        /// </summary>
        /// <param name="message">Message to log.</param>
        internal static void LogCritical(string message) => Log(message, LogSeverity.Critical);
    }    
}
