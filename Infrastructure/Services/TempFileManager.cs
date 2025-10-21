using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace HK_AREA_SEARCH.Infrastructure.Services
{
    /// <summary>
    /// 临时文件管理器
    /// </summary>
    public class TempFileManager
    {
        private readonly List<string> _tempFiles;
        private readonly string _tempDirectory;

        public TempFileManager()
        {
            _tempFiles = new List<string>();
            
            // 创建插件专用的临时目录
            _tempDirectory = Path.Combine(Path.GetTempPath(), "HK_AREA_SEARCH");
            if (!Directory.Exists(_tempDirectory))
            {
                Directory.CreateDirectory(_tempDirectory);
            }
        }

        /// <summary>
        /// 创建临时文件
        /// </summary>
        /// <param name="fileName">文件名</param>
        /// <returns>临时文件完整路径</returns>
        public string CreateTempFile(string fileName)
        {
            string fullPath = Path.Combine(_tempDirectory, fileName);
            
            // 如果文件名冲突，添加时间戳
            if (File.Exists(fullPath))
            {
                string fileNameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
                string extension = Path.GetExtension(fileName);
                string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
                fullPath = Path.Combine(_tempDirectory, $"{fileNameWithoutExt}_{timestamp}{extension}");
            }

            return fullPath;
        }

        /// <summary>
        /// 注册临时文件
        /// </summary>
        /// <param name="filePath">文件路径</param>
        public void RegisterTempFile(string filePath)
        {
            if (!string.IsNullOrWhiteSpace(filePath) && !_tempFiles.Contains(filePath))
            {
                _tempFiles.Add(filePath);
            }
        }

        /// <summary>
        /// 清理所有临时文件
        /// </summary>
        public void CleanupAll()
        {
            foreach (string tempFile in _tempFiles.ToList())
            {
                try
                {
                    if (File.Exists(tempFile))
                    {
                        File.Delete(tempFile);
                    }
                    _tempFiles.Remove(tempFile);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"删除临时文件失败 {tempFile}: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// 按模式清理
        /// </summary>
        /// <param name="pattern">文件名模式</param>
        public void CleanupByPattern(string pattern)
        {
            foreach (string tempFile in _tempFiles.Where(f => Path.GetFileName(f).Contains(pattern)).ToList())
            {
                try
                {
                    if (File.Exists(tempFile))
                    {
                        File.Delete(tempFile);
                    }
                    _tempFiles.Remove(tempFile);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"删除临时文件失败 {tempFile}: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// 获取注册的临时文件列表
        /// </summary>
        public List<string> GetTempFiles => new List<string>(_tempFiles);
    }
}