namespace HK_AREA_SEARCH.Configuration
{
    /// <summary>
    /// 分析配置
    /// </summary>
    public class AnalysisConfig
    {
        /// <summary>
        /// 最大距离参数（米）
        /// </summary>
        public double MaxDistance { get; set; } = 10000; // 10公里

        /// <summary>
        /// 分类数量
        /// </summary>
        public int NumClasses { get; set; } = 10;

        /// <summary>
        /// 是否启用详细日志
        /// </summary>
        public bool EnableDetailedLogging { get; set; } = false;

        /// <summary>
        /// 栅格像元大小
        /// </summary>
        public double CellSize { get; set; } = 10.0; // 10米

        /// <summary>
        /// 简化容差
        /// </summary>
        public double SimplifyTolerance { get; set; } = 1.0; // 1米
    }
}