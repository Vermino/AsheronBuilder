// AsheronBuilder.Core/Utils/Logger.cs

using System;
using System.IO;

namespace AsheronBuilder.Core.Utils
{
    public static class Logger
    {
        private static readonly string LogFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AsheronBuilder.log");

        public static void Log(string message, LogLevel level = LogLevel.Info)
        {
            string logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{level}] {message}";
            Console.WriteLine(logEntry);

            try
            {
                File.AppendAllText(LogFilePath, logEntry + Environment.NewLine);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to write to log file: {ex.Message}");
            }
        }

        public static void LogError(string message, Exception ex)
        {
            Log($"{message} - Exception: {ex.Message}", LogLevel.Error);
            Log($"StackTrace: {ex.StackTrace}", LogLevel.Error);
        }
    }

    public enum LogLevel
    {
        Debug,
        Info,
        Warning,
        Error
    }
}