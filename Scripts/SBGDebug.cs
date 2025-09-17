using UnityEngine;
using System;
using System.IO;

#region SBG Debug
public static class SBGDebug
{
    private static readonly object _lockObj = new object();
    private static string _logFilePath;
    private static bool _isInitialized = false;
    private static readonly int _maxLogFileSize = 10 * 1024 * 1024; // 10MB max file size

    public enum LogLevel
    {
        Error,
        Warning,
        Info,
        Debug,
        Verbose
    }

    public static LogLevel CurrentLogLevel { get; set; } = LogLevel.Info;

    /// <summary>
    /// Initializes the logging system
    /// </summary>
    private static void Initialize()
    {
        if (_isInitialized) return;

        try
        {
            string logDirectory = Path.Combine(Application.persistentDataPath, "Logs");

            if (!Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }

            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            _logFilePath = Path.Combine(logDirectory, $"BinaryBetrayal_{timestamp}.log");

            // Write header to log file
            string header = $"=== Binary Betrayal Log Started at {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} ===\n" +
                           $"Game Version: {Application.version}\n" +
                           $"Platform: {Application.platform}\n" +
                           $"System: {SystemInfo.operatingSystem}\n" +
                           $"Device: {SystemInfo.deviceModel}\n" +
                           $"=====================================================\n";

            File.WriteAllText(_logFilePath, header);
            _isInitialized = true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to initialize logging: {ex.Message}");
        }
    }

    /// <summary>
    /// Writes a message to the log file
    /// </summary>
    private static void WriteToFile(string message)
    {
        if (!_isInitialized)
        {
            Initialize();
        }

        try
        {
            lock (_lockObj)
            {
                // Check if log file exceeds max size, rotate if needed
                if (File.Exists(_logFilePath) && new FileInfo(_logFilePath).Length > _maxLogFileSize)
                {
                    RotateLogFile();
                }

                // Append message to log file
                using (StreamWriter writer = File.AppendText(_logFilePath))
                {
                    writer.WriteLine(message);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to write to log file: {ex.Message}");
        }
    }

    /// <summary>
    /// Rotates the log file when it gets too large
    /// </summary>
    private static void RotateLogFile()
    {
        try
        {
            string directory = Path.GetDirectoryName(_logFilePath);
            string fileNameWithoutExt = Path.GetFileNameWithoutExtension(_logFilePath);
            string extension = Path.GetExtension(_logFilePath);
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");

            string newPath = Path.Combine(directory, $"{fileNameWithoutExt}_{timestamp}{extension}");
            File.Move(_logFilePath, newPath);

            // Create a new log file
            string header = $"=== Binary Betrayal Log Continued at {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} ===\n";
            File.WriteAllText(_logFilePath, header);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to rotate log file: {ex.Message}");
        }
    }

    /// <summary>
    /// Formats the log message with timestamp and context
    /// </summary>
    private static string FormatLogMessage(string message, string context, LogLevel level)
    {
        string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
        string threadId = System.Threading.Thread.CurrentThread.ManagedThreadId.ToString();
        return $"[{timestamp}][{level}][Thread:{threadId}] {message} | Context: {context}";
    }

    public static void LogError(string message, string context)
    {
        if (CurrentLogLevel >= LogLevel.Error)
        {
            string formattedMessage = FormatLogMessage(message, context, LogLevel.Error);
            Debug.LogError($"<color=red>{message}</color> | <color=yellow>{context}</color>");
            WriteToFile(formattedMessage);
        }
    }

    public static void LogWarning(string message, string context)
    {
        if (CurrentLogLevel >= LogLevel.Warning)
        {
            string formattedMessage = FormatLogMessage(message, context, LogLevel.Warning);
            Debug.LogWarning($"<color=yellow>{message}</color> | <color=yellow>{context}</color>");
            WriteToFile(formattedMessage);
        }
    }

    public static void LogInfo(string message, string context)
    {
        if (CurrentLogLevel >= LogLevel.Info)
        {
            string formattedMessage = FormatLogMessage(message, context, LogLevel.Info);
            Debug.Log($"<color=blue>{message}</color> | <color=yellow>{context}</color>");
            WriteToFile(formattedMessage);
        }
    }

    public static void LogDebug(string message, string context)
    {
        if (CurrentLogLevel >= LogLevel.Debug)
        {
            string formattedMessage = FormatLogMessage(message, context, LogLevel.Debug);
            Debug.Log($"<color=green>{message}</color> | <color=yellow>{context}</color>");
            WriteToFile(formattedMessage);
        }
    }

    public static void LogVerbose(string message, string context)
    {
        if (CurrentLogLevel >= LogLevel.Verbose)
        {
            string formattedMessage = FormatLogMessage(message, context, LogLevel.Verbose);
            Debug.Log($"<color=gray>{message}</color> | <color=yellow>{context}</color>");
            WriteToFile(formattedMessage);
        }
    }

    public static void LogEvent(string message, string context)
    {
        if (CurrentLogLevel >= LogLevel.Info)
        {
            string formattedMessage = FormatLogMessage($"EVENT: {message}", context, LogLevel.Info);
            Debug.Log($"<color=purple>EVENT: {message}</color> | <color=yellow>{context}</color>");
            WriteToFile(formattedMessage);
        }
    }

    /// <summary>
    /// Logs an exception with stack trace
    /// </summary>
    public static void LogException(Exception exception, string context)
    {
        if (CurrentLogLevel >= LogLevel.Error)
        {
            string message = $"Exception: {exception.Message}\nStackTrace: {exception.StackTrace}";
            string formattedMessage = FormatLogMessage(message, context, LogLevel.Error);
            Debug.LogException(exception);
            WriteToFile(formattedMessage);
        }
    }

    /// <summary>
    /// Sets the current log level
    /// </summary>
    public static void SetLogLevel(LogLevel level)
    {
        CurrentLogLevel = level;
        LogInfo($"Log level set to {level}", "SBGDebug");
    }
    #endregion
}