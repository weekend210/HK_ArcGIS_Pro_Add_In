using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Core.Geoprocessing;
using ArcGIS.Desktop.Mapping;
using HK_AREA_SEARCH.Models;
using HK_AREA_SEARCH.Infrastructure.Services;
using HK_AREA_SEARCH.Infrastructure.Helpers;
using System.Reflection;


namespace HK_AREA_SEARCH.Distance
{
    /// <summary>
    /// /*栅格重分类器*/
    /// </summary>
    public class RasterReclassifier
    {
        private readonly TempFileManager _tempFileManager;

        public RasterReclassifier(TempFileManager tempFileManager)
        {
            _tempFileManager = tempFileManager ?? throw new ArgumentNullException(nameof(tempFileManager));
        }

        /// <summary>
        /// 创建等间隔分类
        /// </summary>
        /// <param name="inputRasterPath">输入栅格路径</param>
        /// <param name="numClasses">分类数量</param>
        /// <param name="invertValues">是否反转分类值</param>
        /// <returns>重分类后的栅格路径</returns>
        public async Task<string> CreateEqualIntervalClasses(string inputRasterPath, int numClasses, POIDataItem poiItem)
        {
            return await QueuedTask.Run(async () =>
            {
                try
                {
                    string outputRasterPath = _tempFileManager.CreateTempFile("reclassification.tif");
                    _tempFileManager.RegisterTempFile(outputRasterPath);

                    // 直接使用重分类工具进行等间隔分类
                    // 构建等间隔重分类表达式
                    string reclassExpression = await BuildEqualIntervalExpression(poiItem, numClasses);

                    var parameters = Geoprocessing.MakeValueArray(
                        inputRasterPath,      // 输入栅格
                        "VALUE",             // 重分类字段（注意是大写）
                        reclassExpression,   // 重映射表达式
                        outputRasterPath,    // 输出栅格
                        "DATA",              // 缺失值处理
                        ""                   // WHERE子句
                    );

                    // 环境变量
                    var environments = Geoprocessing.MakeEnvironmentArray(
                        cellSize: null,
                        extent: null,
                        mask: null,
                        outputCoordinateSystem: null,
                        scratchWorkspace: null,
                        workspace: null
                    );

                    // 参数设置
                    GPExecuteToolFlags executeFlags = GPExecuteToolFlags.AddToHistory | GPExecuteToolFlags.AddOutputsToMap;


                    // 执行重分类工具
                    var result = await Geoprocessing.ExecuteToolAsync("sa.Reclassify", parameters, environments, null, null, executeFlags);

                    if (result.IsFailed)
                    {
                        // 将错误消息列表转换为字符串
                        string errorMessages = string.Join("; ", result.ErrorMessages);
                        System.Diagnostics.Debug.WriteLine($"等间隔分类失败: {errorMessages}");
                    }

                    return outputRasterPath;
                }
                catch (Exception ex)
                {
                    throw new Exception($"创建等间隔分类时发生错误: {ex.Message}", ex);
                }
            });
        }

        /// <summary>
        /// /*构建等间隔重分类表达式*/
        /// </summary>
        private async Task<string> BuildEqualIntervalExpression(POIDataItem poiItem, int numClasses)
        {
            return await QueuedTask.Run(() =>
            {
                int distance = poiItem?.Distance != null ? poiItem.Distance.Value : 1000;   // 默认距离为1000米

                var reclassRules = new List<string>();
                double minValue = 0;
                double maxValue = Math.Abs(distance);
                double interval = (maxValue - minValue) / numClasses;

                if (distance > 0)
                {
                    // 如果距离为正，分类值从10到1
                    for (int i = 0; i < numClasses; i++)
                    {
                        double fromValue = minValue + (i * interval);
                        double toValue = (i == numClasses - 1) ? maxValue : minValue + ((i + 1) * interval); // 确保最后一个区间包含最大值
                        int classValue = numClasses - i; // 反转分类值

                        reclassRules.Add($"{fromValue} {toValue} {classValue}");
                    }
                }
                else
                {
                    // 如果距离为负，分类值从1到10
                    for (int i = 0; i < numClasses; i++)
                    {

                        double fromValue = minValue + (i * interval);
                        double toValue = (i == numClasses - 1) ? maxValue : minValue + ((i + 1) * interval); // 确保最后一个区间包含最大值
                        int classValue = i + 1;

                        reclassRules.Add($"{fromValue} {toValue} {classValue}");
                    }
                }

                return string.Join(";", reclassRules);
            });
        }

        /// <summary>
        /// 创建自定义分类
        /// </summary>
        /// <param name="inputRasterPath">输入栅格路径</param>
        /// <param name="classItems">分类项列表</param>
        /// <param name="invertValues">是否反转分类值</param>
        /// <returns>重分类后的栅格路径</returns>
        public async Task<string> CreateCustomClasses(string inputRasterPath, List<IntervalClassItem> classItems, POIDataItem poiItem)
        {
            var reclassRules = new Dictionary<double, double>();

            // 将IntervalClassItem转换为重分类规则
            foreach (var item in classItems)
            {
                // 构建正确的重分类规则
                // 每个规则格式: 起始值 结束值 新值
                reclassRules[item.StartValue] = item.ClassValue;
            }

            // 构建重分类表达式
            string remapExpression = BuildRemapExpression(reclassRules);

            return await QueuedTask.Run(async () =>
            {
                try
                {
                    string outputRasterPath = _tempFileManager.CreateTempFile("custom_classification.tif");
                    _tempFileManager.RegisterTempFile(outputRasterPath);

                    // 使用重分类工具
                    var parameters = Geoprocessing.MakeValueArray(
                        inputRasterPath,      // 输入栅格
                        "VALUE",             // 重分类字段（注意是大写）
                        remapExpression,     // 重映射表达式
                        outputRasterPath,    // 输出栅格
                        "DATA",              // 缺失值处理
                        ""                   // WHERE子句
                    );

                    // 执行重分类工具
                    var result = await Geoprocessing.ExecuteToolAsync("sa.Reclassify", parameters,
                        // 设置合适的环境参数
                        Geoprocessing.MakeEnvironmentArray(
                            cellSize: null,
                            extent: null,
                            mask: null,
                            outputCoordinateSystem: null,
                            scratchWorkspace: null,
                            workspace: null
                        ));

                    if (result.IsFailed)
                    {
                        // 将错误消息列表转换为字符串
                        string errorMessages = string.Join("; ", result.ErrorMessages);
                        throw new Exception($"自定义分类失败: {errorMessages}");
                    }

                    return outputRasterPath;
                }
                catch (Exception ex)
                {
                    throw new Exception($"创建自定义分类时发生错误: {ex.Message}", ex);
                }
            });
        }

        /// <summary>
        /// 构建重分类表达式
        /// </summary>
        private string BuildRemapExpression(Dictionary<double, double> reclassRules)
        {
            if (reclassRules == null || reclassRules.Count == 0)
                return "";

            // 为ArcGIS重分类工具构建正确的重映射字符串
            // 格式: "from1 to1;from2 to2;..." 注意使用分号分隔
            var remapPairs = new List<string>();

            foreach (var rule in reclassRules)
            {
                // 对于单个值的映射，需要指定相同的起始和结束值
                remapPairs.Add($"{rule.Key} {rule.Key} {rule.Value}");
            }

            // 用分号连接所有重映射对
            return string.Join(";", remapPairs);
        }
    }
}