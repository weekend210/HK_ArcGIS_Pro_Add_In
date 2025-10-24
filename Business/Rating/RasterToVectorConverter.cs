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

namespace HK_AREA_SEARCH.Rating
{
    /// <summary>
    /// 栅格转矢量转换器
    /// </summary>
    public class RasterToVectorConverter
    {
        private readonly TempFileManager _tempFileManager;

        public RasterToVectorConverter(TempFileManager tempFileManager)
        {
            _tempFileManager = tempFileManager ?? throw new ArgumentNullException(nameof(tempFileManager));
        }

        /// <summary>
        /// 执行转换
        /// </summary>
        /// <param name="inputRasterPath">输入栅格路径</param>
        /// <returns>转换后的矢量路径</returns>
        public async Task<string> ConvertAsync(string inputRasterPath)
        {
            return await QueuedTask.Run(async () =>
            {
                try
                {
                    string outputVectorPath = _tempFileManager.CreateTempFile("rating.shp");
                    _tempFileManager.RegisterTempFile(outputVectorPath);

                    // 使用栅格转面工具
                    var parameters = Geoprocessing.MakeValueArray(
                        inputRasterPath,    // 输入栅格
                        outputVectorPath,   // 输出面要素类
                        "DATA",             // 简化类型（DATA表示简化几何）
                        "VALUE",            // 字段名
                        "1"                 // 生成多边形ID
                    );

                    var result = await Geoprocessing.ExecuteToolAsync("conversion.RasterToPolygon", parameters);

                    if (result.IsFailed)
                    {
                        // 将错误消息列表转换为字符串
                        string errorMessages = string.Join("; ", result.ErrorMessages);
                        throw new Exception($"栅格转矢量失败: {errorMessages}");
                    }

                    // 可选：简化面要素
                    //outputVectorPath = await SimplifyPolygons(outputVectorPath);

                    return outputVectorPath;
                }
                catch (Exception ex)
                {
                    throw new Exception($"执行栅格转矢量时发生错误: {ex.Message}", ex);
                }
            });
        }

        /// <summary>
        /// 简化面要素
        /// </summary>
        /// <param name="inputVectorPath">输入矢量路径</param>
        /// <returns>简化后的矢量路径</returns>
        public async Task<string> SimplifyPolygons(string inputVectorPath)
        {
            return await QueuedTask.Run(async () =>
            {
                try
                {
                    string outputVectorPath = _tempFileManager.CreateTempFile("simplified.shp");
                    _tempFileManager.RegisterTempFile(outputVectorPath);

                    // 使用简化面工具
                    var parameters = Geoprocessing.MakeValueArray(
                        inputVectorPath,
                        outputVectorPath,
                        "POINT_REMOVE",     // 简化算法
                        "10 Meters",        // 容差
                        "0 Meters",         // 最大偏移量
                        "0 SquareMeters"    // 最大面积
                    );

                    var result = await Geoprocessing.ExecuteToolAsync("cartography.SimplifyPolygon", parameters);

                    if (result.IsFailed)
                    {
                        // 如果简化失败，返回原始路径
                        return inputVectorPath;
                    }

                    return outputVectorPath;
                }
                catch (Exception ex)
                {
                    // 如果简化失败，返回原始路径
                    System.Diagnostics.Debug.WriteLine($"简化面要素失败: {ex.Message}");
                    return inputVectorPath;
                }
            });
        }

        /// <summary>
        /// 保留字段
        /// </summary>
        /// <param name="inputVectorPath">输入矢量路径</param>
        /// <param name="fieldNames">字段名列表</param>
        /// <returns>处理后的矢量路径</returns>
        public async Task<string> PreserveField(string inputVectorPath, string fieldName)
        {
            // 此方法在基础实现中主要作为标记，实际字段保留通常在转换过程中设置
            await Task.CompletedTask;
            return inputVectorPath;
        }
    }
}