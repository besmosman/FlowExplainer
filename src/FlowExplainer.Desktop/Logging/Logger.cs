using FlowExplainer.Logging;

namespace FlowExplainer
{
    public static class Logger
    {
        private static List<LogEntry> logs = new();
        public static IEnumerable<LogEntry> GetLogs() => logs;
        public static int LastEntryID { get; private set; }
        public static void Clean(int maxLogs)
        {
            if (logs.Count - maxLogs > 0)
                logs.RemoveRange(0, logs.Count - maxLogs);
        }

        private static void Log(LogLevel level, object message)
        {
            logs.Add(new LogEntry
            {
                LogLevel = level,
                Message = message?.ToString() ?? "?",
                Time = DateTime.Now.TimeOfDay,
            });
            LastEntryID++;
        }

        public static void LogDebug(object message)
        {
            Log(LogLevel.Debug, message);
        }

        public static void LogWarn(object message)
        {
            Log(LogLevel.Warning, message);
        }

        public static void LogMessage(object message)
        {
            Log(LogLevel.Message, message);
        }

    }
}