using System;
using System.IO;

namespace HK_AREA_SEARCH.Configuration
{
    /// <summary>
    /// 路径配置
    /// </summary>
    public class PathConfig
    {
        /// <summary>
        /// 临时文件目录
        /// </summary>
        public string TempDirectory { get; set; } = Path.Combine(Path.GetTempPath(), "HK_AREA_SEARCH");

        /// <summary>
        /// 输出目录
        /// </summary>
        public string OutputDirectory { get; set; } = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

        /// <summary>
        /// 缓存目录
        /// </summary>
        public string CacheDirectory { get; set; } = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "HK_AREA_SEARCH", 
            "cache");

        /// <summary>
        /// 日志目录
        /// </summary>
        public string LogDirectory { get; set; } = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "HK_AREA_SEARCH", 
            "logs");
    }
}