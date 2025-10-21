using System;
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

namespace HK_AREA_SEARCH.Distance
{
    /// <summary>
    /// NoData值处理器
    /// </summary>
    public class NoDataHandler
    {
        private readonly TempFileManager _tempFileManager;

        public NoDataHandler(TempFileManager tempFileManager)
        {
            _tempFileManager = tempFileManager ?? throw new ArgumentNullException(nameof(tempFileManager));
        }

        /// <summary>
        /// 替换NoData值
        /// </summary>
        /// <param name="inputRasterPath">输入栅格路径</param>
        /// <param name="newValue">新值</param>
        /// <returns>处理后的栅格路径</returns>
        public async Task<string> ReplaceNoDataAsync(string inputRasterPath, double newValue)
        {
            return await QueuedTask.Run(async () =>
            {
                try
                {
                    string outputRasterPath = _tempFileManager.CreateTempFile("nodata_processed.tif");
                    _tempFileManager.RegisterTempFile(outputRasterPath);

                    // 使用Con函数处理NoData值: Con(IsNull("input"), newValue, "input")
                    // 栅格计算器参数: in_rasters, expression, out_raster
                    var parameters = Geoprocessing.MakeValueArray(
                        new[] { inputRasterPath },  // 输入栅格数组
                        $"Con(IsNull(\"{Path.GetFileNameWithoutExtension(inputRasterPath)}\"), {newValue}, \"{Path.GetFileNameWithoutExtension(inputRasterPath)}\")",  // 表达式
                        outputRasterPath  // 输出栅格
                    );

                    // 执行栅格计算器工具
                    var result = await Geoprocessing.ExecuteToolAsync("sa.RasterCalculator", parameters);

                    if (result.IsFailed)
                    {
                        // 将错误消息列表转换为字符串
                        string errorMessages = string.Join("; ", result.ErrorMessages);
                        throw new Exception($"处理NoData值失败: {errorMessages}");
                    }

                    return outputRasterPath;
                }
                catch (Exception ex)
                {
                    throw new Exception($"处理NoData值时发生错误: {ex.Message}", ex);
                }
            });
        }

        /// <summary>
        /// 将NoData转为指定值
        /// </summary>
        /// <param name="inputRasterPath">输入栅格路径</param>
        /// <param name="targetValue">目标值</param>
        /// <returns>处理后的栅格路径</returns>
        public async Task<string> ConvertNoDataToValue(string inputRasterPath, double targetValue)
        {
            return await ReplaceNoDataAsync(inputRasterPath, targetValue);
        }
    }
}