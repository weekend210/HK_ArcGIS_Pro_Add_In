using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Core.Geoprocessing;

namespace HK_AREA_SEARCH.Infrastructure.Helpers
{
    /// <summary>
    /// 地理处理辅助
    /// </summary>
    public static class GeoprocessingHelper
    {
        /// <summary>
        /// 执行GP工具
        /// </summary>
        /// <param name="toolName">工具名称</param>
        /// <param name="parameters">参数数组</param>
        /// <returns>执行结果</returns>
        public static async Task<IGPResult> ExecuteToolAsync(string toolName, object[] parameters)
        {
            return await QueuedTask.Run(async () =>
            {
                try
                {
                    var paramValues = Geoprocessing.MakeValueArray(parameters);
                    var result = await Geoprocessing.ExecuteToolAsync(toolName, paramValues);
                    return result as IGPResult;
                }
                catch (Exception ex)
                {
                    throw new Exception($"执行地理处理工具 '{toolName}' 失败: {ex.Message}", ex);
                }
            });
        }

        /// <summary>
        /// 获取工具参数
        /// </summary>
        /// <param name="toolName">工具名称</param>
        /// <returns>参数字典</returns>
        public static Dictionary<string, object> GetToolParameters(string toolName)
        {
            var parameters = new Dictionary<string, object>();

            // 根据工具名称返回相应的参数信息
            switch (toolName.ToLower())
            {
                case "analysis.intersect":
                    parameters.Add("in_features", "");
                    parameters.Add("out_feature_class", "");
                    parameters.Add("join_attributes", "ALL");
                    break;
                case "analysis.erase":
                    parameters.Add("in_features", "");
                    parameters.Add("erase_features", "");
                    parameters.Add("out_feature_class", "");
                    break;
                case "conversion.rastertopolygon":
                    parameters.Add("in_raster", "");
                    parameters.Add("out_polygon_features", "");
                    parameters.Add("simplify", "DATA");
                    break;
                case "sa.eucdistance":
                    parameters.Add("in_source_data", "");
                    parameters.Add("out_distance_raster", "");
                    break;
                case "sa.reclassify":
                    parameters.Add("in_raster", "");
                    parameters.Add("reclass_field", "");
                    parameters.Add("remap", "");
                    parameters.Add("out_raster", "");
                    break;
                default:
                    parameters.Add("tool_name", toolName);
                    break;
            }

            return parameters;
        }

        /// <summary>
        /// 检查工具状态
        /// </summary>
        /// <param name="result">工具执行结果</param>
        /// <returns>是否成功</returns>
        public static bool CheckToolStatus(IGPResult result)
        {
            if (result == null)
                return false;

            return !result.IsFailed;
        }
    }
}