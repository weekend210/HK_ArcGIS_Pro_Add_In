using System;
using System.IO;
using System.Threading.Tasks;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Core.Geoprocessing;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Mapping;
using HK_AREA_SEARCH.Infrastructure.Services;
using HK_AREA_SEARCH.Infrastructure.Helpers;
using System.Globalization;
//using System.Diagnostics; // 引入 Debug.WriteLine

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
                    // Raster(r"")构建绝对路径
                    string newValueString = newValue.ToString(CultureInfo.InvariantCulture);
                    string Expression = $"Con(IsNull(Raster(r\"{inputRasterPath}\")), {newValueString}, Raster(r\"{inputRasterPath}\"))";

                    var parameters = Geoprocessing.MakeValueArray(
                        Expression,  // 表达式
                        outputRasterPath  // 输出栅格
                    );

                    MessageBox.Show(Expression, "表达式");

                    // ***** 在控制台输出 Expression 用于调试 *****
                    //Debug.WriteLine($"[DEBUG-RasterCalculator] 输入路径: {inputRasterPath}");
                    //Debug.WriteLine($"[DEBUG-RasterCalculator] 表达式: {Expression}");
                    // **********************************************

                    // 环境变量
                    var environments = Geoprocessing.MakeEnvironmentArray(
                        cellSize: null,
                        extent: null,
                        mask: null,
                        outputCoordinateSystem: null,
                        scratchWorkspace: null,
                        workspace: null
                    );

                    // 执行栅格计算器工具
                    var result = await Geoprocessing.ExecuteToolAsync("RasterCalculator_sa", parameters, environments);

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