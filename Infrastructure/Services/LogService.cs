using System;
using System.IO;

namespace HK_AREA_SEARCH.Infrastructure.Services
{
    /// <summary>
    /// 日志服务
    /// </summary>
    public class LogService
    {
        private static readonly string LogFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "HK_AREA_SEARCH", 
            "logs", 
            $"log_{DateTime.Now:yyyyMMdd}.txt");

        static LogService()
        {
            // 确保日志目录存在
            string logDir = Path.GetDirectoryName(LogFilePath);
            if (!Directory.Exists(logDir))
            {
                Directory.CreateDirectory(logDir);
            }
        }

        /// <summary>
        /// 记录信息
        /// </summary>
        /// <param name="message">消息</param>
        public static void LogInfo(string message)
        {
            WriteLog("INFO", message);
        }

        /// <summary>
        /// 记录警告
        /// </summary>
        /// <param name="message">消息</param>
        public static void LogWarning(string message)
        {
            WriteLog("WARNING", message);
        }

        /// <summary>
        /// 记录错误
        /// </summary>
        /// <param name="message">消息</param>
        public static void LogError(string message)
        {
            WriteLog("ERROR", message);
        }

        /// <summary>
        /// 记录进度
        /// </summary>
        /// <param name="message">消息</param>
        public static void LogProgress(string message)
        {
            WriteLog("PROGRESS", message);
        }

        /// <summary>
        /// 写入日志
        /// </summary>
        /// <param name="level">日志级别</param>
        /// <param name="message">消息</param>
        private static void WriteLog(string level, string message)
        {
            try
            {
                string logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{level}] {message}";
                File.AppendAllText(LogFilePath, logEntry + Environment.NewLine);
            }
            catch
            {
                // 避免日志记录本身导致问题
            }
        }
    }
}