using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Core.Geoprocessing;
using ArcGIS.Desktop.Mapping;
using HK_AREA_SEARCH.Infrastructure.Services;
using HK_AREA_SEARCH.Infrastructure.Helpers;
//using System.Diagnostics; // 引入 Debug.WriteLine

namespace HK_AREA_SEARCH.Rating
{
    /// <summary>
    /// 栅格计算器
    /// </summary>
    public class RasterCalculator
    {
        private readonly TempFileManager _tempFileManager;

        public RasterCalculator(TempFileManager tempFileManager)
        {
            _tempFileManager = tempFileManager ?? throw new ArgumentNullException(nameof(tempFileManager));
        }

        /// <summary>
        /// 加权求和
        /// </summary>
        /// <param name="rasterPaths">栅格路径字典</param>
        /// <param name="weights">权重字典</param>
        /// <returns>加权求和结果栅格路径</returns>
        public async Task<string> WeightedSumAsync(Dictionary<string, string> rasterPaths, Dictionary<string, double> weights)
        {
            return await QueuedTask.Run(async () =>
            {
                try
                {
                    string outputRasterPath = _tempFileManager.CreateTempFile("rating.tif");
                    _tempFileManager.RegisterTempFile(outputRasterPath);

                    // 构建加权求和表达式
                    // 计算表达式伪代码：“（结果栅格1* 权重1）+（结果栅格2* 权重2）+（结果栅格3* 权重3）+…”
                    string expression = BuildWeightedSumExpression(rasterPaths, weights);

                    // ***** 在控制台输出 Expression 用于调试 *****
                    //Debug.WriteLine("-----[DEBUG-RasterCalculator]-----");
                    //Debug.WriteLine($"[DEBUG-RasterCalculator] 表达式: {expression}");
                    // **********************************************

                    var parameters = Geoprocessing.MakeValueArray(
                        expression,
                        outputRasterPath
                    );

                    var result = await Geoprocessing.ExecuteToolAsync("sa.RasterCalculator", parameters);

                    if (result.IsFailed)
                    {
                        // 将错误消息列表转换为字符串
                        string errorMessages = string.Join("; ", result.ErrorMessages);
                        throw new Exception($"加权求和计算失败: {errorMessages}");
                    }

                    return outputRasterPath;
                }
                catch (Exception ex)
                {
                    throw new Exception($"执行加权求和时发生错误: {ex.Message}", ex);
                }
            });
        }

        /// <summary>
        /// 构建计算表达式
        /// </summary>
        private string BuildWeightedSumExpression(Dictionary<string, string> rasterPaths, Dictionary<string, double> weights)
        {
            var expressionParts = new List<string>();

            foreach (var kvp in rasterPaths)
            {
                if (weights.ContainsKey(kvp.Key))
                {
                    double weight = weights[kvp.Key];
                    expressionParts.Add($"({weight} * Raster(r\"{kvp.Value}\"))");
                }
            }

            return string.Join(" + ", expressionParts);
        }
    }
}