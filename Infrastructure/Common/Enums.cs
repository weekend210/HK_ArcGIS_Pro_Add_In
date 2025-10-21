namespace HK_AREA_SEARCH.Common
{
    /// <summary>
    /// 数据类型枚举
    /// </summary>
    public enum DataType
    {
        Vector,
        Raster,
        Unknown
    }

    /// <summary>
    /// 分类方法枚举
    /// </summary>
    public enum ClassificationMethod
    {
        EqualInterval,
        Custom
    }

    /// <summary>
    /// 处理状态枚举
    /// </summary>
    public enum ProcessStatus
    {
        NotStarted,
        InProgress,
        Completed,
        Failed
    }

    /// <summary>
    /// 色带类型枚举
    /// </summary>
    public enum ColorRampType
    {
        Preset,
        Custom,
        MultiPart
    }

    /// <summary>
    /// 颜色方案算法枚举
    /// </summary>
    public enum ColorSchemeAlgorithm
    {
        HueSaturationValue,
        RedWhiteBlue,
        YellowOrangeRed
    }
}