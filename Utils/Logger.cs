namespace FFBWheelProperties.Utils
{
    public static class Logger
    {
        private static readonly string LogPath;
        private static readonly object LockObject = new object();
        
        static Logger()
        {
            var appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "FFBWheelProperties"
            );
            
            try
            {
                Directory.CreateDirectory(appDataPath);
                LogPath = Path.Combine(appDataPath, "app.log");
            }
            catch
            {
                LogPath = Path.Combine(Path.GetTempPath(), "FFBWheelProperties_app.log");
            }
        }
        
        public static void Log(string message)
        {
            try
            {
                lock (LockObject)
                {
                    var logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] {message}";
                    File.AppendAllText(LogPath, logEntry + Environment.NewLine);
                    
                    // Also output to debug console in debug builds
                    #if DEBUG
                    System.Diagnostics.Debug.WriteLine(logEntry);
                    #endif
                }
            }
            catch
            {
                // Ignore logging errors to prevent cascading failures
            }
        }
        
        public static void LogException(Exception ex, string context = "")
        {
            var message = string.IsNullOrEmpty(context) 
                ? $"Exception: {ex.Message}\nStack Trace: {ex.StackTrace}"
                : $"Exception in {context}: {ex.Message}\nStack Trace: {ex.StackTrace}";
            
            Log(message);
        }
        
        public static string GetLogPath() => LogPath;
        
        public static void ClearLog()
        {
            try
            {
                lock (LockObject)
                {
                    if (File.Exists(LogPath))
                    {
                        File.Delete(LogPath);
                    }
                }
            }
            catch
            {
                // Ignore errors
            }
        }
    }
}
