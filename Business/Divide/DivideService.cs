using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using HK_AREA_SEARCH.Infrastructure.Services;

namespace HK_AREA_SEARCH.Divide
{
    /// <summary>
    /// 可建设土地划分服务实现
    /// </summary>
    public class DivideService : IDivideService
    {
        private readonly TempFileManager _tempFileManager;

        public DivideService(TempFileManager tempFileManager)
        {
            _tempFileManager = tempFileManager ?? throw new ArgumentNullException(nameof(tempFileManager));
        }

        /// <summary>
        /// 执行可建设土地划分
        /// </summary>
        /// <param name="analysisAreaPath">分析区域路径</param>
        /// <param name="constraintPaths">约束条件路径列表</param>
        /// <param name="outputPath">输出路径</param>
        /// <returns>生成的可建设土地文件路径</returns>
        public async Task<string> ExecuteAsync(string analysisAreaPath, List<string> constraintPaths, string outputPath)
        {
            try
            {
                // 验证输入数据
                await ValidateInputs(analysisAreaPath, constraintPaths);

                // 合并约束条件
                string mergedConstraintsPath = await MergeConstraints(constraintPaths);

                // 执行差集运算 - 可建设土地 = 分析区域 - 约束条件
                string resultPath = await PerformDifference(analysisAreaPath, mergedConstraintsPath, outputPath);

                return resultPath;
            }
            catch (Exception ex)
            {
                throw new Exception($"划分可建设土地失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 验证输入数据
        /// </summary>
        private async Task ValidateInputs(string analysisAreaPath, List<string> constraintPaths)
        {
            await Task.Run(() =>
            {
                if (string.IsNullOrWhiteSpace(analysisAreaPath))
                    throw new ArgumentException("分析区域路径不能为空");

                if (constraintPaths == null || constraintPaths.Count == 0)
                    throw new ArgumentException("约束条件路径列表不能为空");

                if (!File.Exists(analysisAreaPath))
                    throw new FileNotFoundException($"分析区域文件不存在: {analysisAreaPath}");

                foreach (var path in constraintPaths)
                {
                    if (!File.Exists(path))
                        throw new FileNotFoundException($"约束条件文件不存在: {path}");
                }
            });
        }

        /// <summary>
        /// 合并约束条件
        /// </summary>
        private async Task<string> MergeConstraints(List<string> constraintPaths)
        {
            var geometryProcessor = new GeometryProcessor();
            return await geometryProcessor.Union(constraintPaths, _tempFileManager);
        }

        /// <summary>
        /// 执行差集运算
        /// </summary>
        private async Task<string> PerformDifference(string analysisAreaPath, string constraintsPath, string outputPath)
        {
            var geometryProcessor = new GeometryProcessor();
            return await geometryProcessor.Difference(analysisAreaPath, constraintsPath, outputPath);
        }
    }
}