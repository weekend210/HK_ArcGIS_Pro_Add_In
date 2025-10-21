using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using HK_AREA_SEARCH.Infrastructure.Services;

namespace HK_AREA_SEARCH.Divide
{
    /// <summary>
    /// 约束条件合并器
    /// </summary>
    public class ConstraintMerger
    {
        /// <summary>
        /// 合并多个约束条件图层
        /// </summary>
        /// <param name="constraintPaths">约束条件路径列表</param>
        /// <param name="tempFileManager">临时文件管理器</param>
        /// <returns>合并后的约束条件路径</returns>
        public async Task<string> MergeMultipleLayers(List<string> constraintPaths, TempFileManager tempFileManager)
        {
            if (constraintPaths == null || constraintPaths.Count == 0)
                throw new ArgumentException("约束条件路径列表不能为空");

            try
            {
                // 使用几何处理器的Union功能
                var geometryProcessor = new GeometryProcessor();
                return await geometryProcessor.Union(constraintPaths, tempFileManager);
            }
            catch (Exception ex)
            {
                throw new Exception($"合并约束条件失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 创建临时图层
        /// </summary>
        /// <param name="tempFileManager">临时文件管理器</param>
        /// <returns>临时图层路径</returns>
        public string CreateTempLayer(TempFileManager tempFileManager)
        {
            string tempPath = tempFileManager.CreateTempFile("TempLayer.shp");
            tempFileManager.RegisterTempFile(tempPath);
            return tempPath;
        }
    }
}