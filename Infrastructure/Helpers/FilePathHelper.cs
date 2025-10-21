using System;
using System.IO;

namespace HK_AREA_SEARCH.Infrastructure.Helpers
{
    /// <summary>
    /// 文件路径辅助
    /// </summary>
    public static class FilePathHelper
    {
        /// <summary>
        /// 生成临时文件路径
        /// </summary>
        /// <param name="fileName">文件名</param>
        /// <returns>临时文件完整路径</returns>
        public static string GetTempFilePath(string fileName)
        {
            string tempPath = Path.Combine(Path.GetTempPath(), "HK_AREA_SEARCH", fileName);
            Directory.CreateDirectory(Path.GetDirectoryName(tempPath));
            return tempPath;
        }

        /// <summary>
        /// 验证路径有效性
        /// </summary>
        /// <param name="path">路径</param>
        /// <returns>是否有效</returns>
        public static bool ValidatePath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return false;

            try
            {
                // 检查路径是否包含无效字符
                if (Path.GetInvalidPathChars().Length > 0)
                {
                    foreach (char c in Path.GetInvalidPathChars())
                    {
                        if (path.Contains(c))
                            return false;
                    }
                }

                // 检查文件名是否有效
                string fileName = Path.GetFileName(path);
                if (!string.IsNullOrEmpty(fileName))
                {
                    foreach (char c in Path.GetInvalidFileNameChars())
                    {
                        if (fileName.Contains(c))
                            return false;
                    }
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 提取文件名（不含扩展名）
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <returns>文件名</returns>
        public static string ExtractFileName(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return string.Empty;

            return Path.GetFileNameWithoutExtension(filePath);
        }
    }
}