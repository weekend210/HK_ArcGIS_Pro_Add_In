using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HK_AREA_SEARCH.Models;

namespace HK_AREA_SEARCH.Distance
{
    /// <summary>
    /// 距离计算服务接口
    /// </summary>
    public interface IDistanceService
    {
        /// <summary>
        /// 执行距离计算流程
        /// </summary>
        /// <param name="poiItems">POI数据项列表</param>
        /// <param name="analysisAreaPath">分析区域路径，用于提取处理范围</param>
        /// <returns>处理后的栅格路径字典</returns>
        Task<Dictionary<string, string>> ExecuteAsync(List<POIDataItem> poiItems, string analysisAreaPath = null);
    }
}