using System;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Core.Geoprocessing;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Core.CIM;
using HK_AREA_SEARCH.Infrastructure.Services;
using HK_AREA_SEARCH.Infrastructure.Helpers;
using HK_AREA_SEARCH.Models;

namespace HK_AREA_SEARCH.Distance
{
    /// <summary>
    /// 欧氏距离计算器
    /// </summary>
    public class EuclideanDistanceCalculator
    {
        private readonly TempFileManager _tempFileManager;
        private double _maxDistance = 1000; // 默认最大距离1千米

        public EuclideanDistanceCalculator(TempFileManager tempFileManager)
        {
            _tempFileManager = tempFileManager ?? throw new ArgumentNullException(nameof(tempFileManager));
        }

        /// <summary>
        /// 执行欧氏距离计算
        /// </summary>
        /// <param name="inputVectorPath">输入矢量路径</param>
        /// <param name="extentEnvelope">分析区域边界，用于设置处理范围</param>
        /// <returns>距离栅格路径</returns>
        public async Task<string> CalculateAsync(string inputVectorPath, Envelope extentEnvelope)
        {
            return await QueuedTask.Run(async () =>
            {
                try
                {
                    // 验证输入路径
                    if (string.IsNullOrWhiteSpace(inputVectorPath))
                    {
                        throw new ArgumentException("输入矢量路径不能为空", nameof(inputVectorPath));
                    }

                    // 检查文件是否存在
                    if (!File.Exists(inputVectorPath))
                    {
                        throw new FileNotFoundException($"输入文件不存在: {inputVectorPath}", inputVectorPath);
                    }

                    // 验证文件扩展名是否为支持的矢量格式
                    string extension = Path.GetExtension(inputVectorPath)?.ToLower();
                    if (extension != ".shp" && extension != ".gdb" && extension != ".kml" && extension != ".geojson")
                    {
                        throw new ArgumentException($"不支持的矢量文件格式: {extension}。支持的格式包括: .shp, .gdb, .kml, .geojson", nameof(inputVectorPath));
                    }

                    // 对于Shapefile，确保有完整的文件集（.shp, .shx, .dbf）
                    if (extension == ".shp")
                    {
                        string shxPath = Path.ChangeExtension(inputVectorPath, ".shx");
                        string dbfPath = Path.ChangeExtension(inputVectorPath, ".dbf");
                        
                        if (!File.Exists(shxPath))
                        {
                            throw new FileNotFoundException($"缺少关联的索引文件: {shxPath}", shxPath);
                        }
                        
                        if (!File.Exists(dbfPath))
                        {
                            throw new FileNotFoundException($"缺少关联的属性文件: {dbfPath}", dbfPath);
                        }
                    }

                    // 创建输出栅格路径
                    string outputRasterPath = _tempFileManager.CreateTempFile("distance.tif");
                    _tempFileManager.RegisterTempFile(outputRasterPath);

                    // 使用ArcGIS Pro的欧氏距离工具
                    // 参数: in_source_data, out_distance_raster, maximum_distance, cell_size
                    var parameters = Geoprocessing.MakeValueArray(
                        inputVectorPath,           // 输入源数据
                        outputRasterPath,          // 输出距离栅格
                        _maxDistance,              // 最大距离（可选）
                        "50"                       // 像元大小（可选）
                    );

                    // 环境变量
                    var environments = Geoprocessing.MakeEnvironmentArray(
                        cellSize: null,           // 像元大小
                        extent: extentEnvelope,   // 使用分析区域的边界范围
                        mask: null,             // 掩膜
                        outputCoordinateSystem: null,  // 输出坐标系
                        scratchWorkspace: null,       // 临时工作空间
                        workspace: null               // 工作空间
                    );
                    
                    // 执行欧氏距离工具 - 使用正确的工具名称
                    var result = await Geoprocessing.ExecuteToolAsync("sa.EucDistance", parameters, environments, null, null, GPExecuteToolFlags.AddToHistory);

                    if (result.IsFailed)
                    {
                        // 获取详细的错误信息
                        var errorMessages = new System.Text.StringBuilder();
                        foreach (var msg in result.ErrorMessages)
                        {
                            errorMessages.AppendLine($"Code: {msg.ErrorCode}, Type: {msg.Type}, Message: {msg.Text}");
                        }
                        
                        // 如果没有具体的错误消息，添加通用错误信息
                        if (errorMessages.Length == 0)
                        {
                            errorMessages.Append("未知错误: 工具执行失败但未返回具体错误信息");
                        }
                        
                        throw new Exception($"欧氏距离计算失败: {errorMessages}");
                    }

                    return outputRasterPath;
                }
                catch (Exception ex)
                {
                    throw new Exception($"计算欧氏距离时发生错误: {ex.Message}", ex);
                }
            });
        }

        /// <summary>
        /// 设置最大距离
        /// </summary>
        /// <param name="maxDistance">最大距离（米）</param>
        public void SetMaxDistance(double maxDistance)
        {
            _maxDistance = maxDistance;
        }

        /// <summary>
        /// 保存结果栅格
        /// </summary>
        /// <param name="rasterPath">栅格路径</param>
        public async Task SaveRaster(string rasterPath)
        {
            // 此方法用于保存栅格，实际实现可能根据需要而有所不同
            await Task.CompletedTask;
        }
    }
}