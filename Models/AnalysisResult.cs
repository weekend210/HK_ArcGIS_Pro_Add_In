using System.Collections.Generic;

namespace HK_AREA_SEARCH.Models
{
    /// <summary>
    /// 分析结果模型
    /// </summary>
    public class AnalysisResult
    {
        /// <summary>
        /// 结果文件路径
        /// </summary>
        public string ResultPath { get; set; }

        /// <summary>
        /// 临时文件列表
        /// </summary>
        public List<string> TempFiles { get; set; } = new List<string>();

        /// <summary>
        /// 是否成功
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// 错误消息
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// 执行时间（秒）
        /// </summary>
        public double ExecutionTime { get; set; }
    }
}