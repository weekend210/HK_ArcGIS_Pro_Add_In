using System;
using System.IO;
using System.Threading.Tasks;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Framework.Threading.Tasks;

namespace HK_AREA_SEARCH.Models
{
    /// <summary>
    /// 空间边界范围模型
    /// </summary>
    public class SpatialBounds
    {
        /// <summary>
        /// 北边界 (最大Y)
        /// </summary>
        public double North { get; set; }
        
        /// <summary>
        /// 南边界 (最小Y)
        /// </summary>
        public double South { get; set; }
        
        /// <summary>
        /// 东边界 (最大X)
        /// </summary>
        public double East { get; set; }
        
        /// <summary>
        /// 西边界 (最小X)
        /// </summary>
        public double West { get; set; }
        
        /// <summary>
        /// 构造函数
        /// </summary>
        public SpatialBounds()
        {
        }
        
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="west">西边界</param>
        /// <param name="south">南边界</param>
        /// <param name="east">东边界</param>
        /// <param name="north">北边界</param>
        public SpatialBounds(double west, double south, double east, double north)
        {
            West = west;
            South = south;
            East = east;
            North = north;
        }
        
        /// <summary>
        /// 从要素类路径获取边界范围
        /// </summary>
        /// <param name="featurePath">要素类路径（如.shp文件路径）</param>
        /// <returns>要素类的边界范围</returns>
        public static async Task<SpatialBounds> GetBoundsFromFeatureClass(string featurePath)
        {
            return await QueuedTask.Run(() =>
            {
                if (string.IsNullOrWhiteSpace(featurePath) || !File.Exists(featurePath))
                {
                    return new SpatialBounds(); // Return an empty bounds if file doesn't exist
                }

                // The proper implementation would use ArcGIS API to read the shapefile extent
                // This is a placeholder that would be implemented in a real application
                // following the same principles as the DistanceService.GetFeatureClassBounds
                // For now, return an empty SpatialBounds as a placeholder
                return new SpatialBounds(); // Placeholder implementation - actual implementation would require proper geodatabase access
            });
        }
        
        /// <summary>
        /// 将SpatialBounds转换为ArcGIS Core Envelope对象
        /// </summary>
        /// <returns>Envelope对象</returns>
        public Envelope ToEnvelope()
        {
            try
            {
                // 使用Web Mercator空间参考系统，这在ArcGIS Pro中是一个常用的投影系统
                var spatialRef = SpatialReferences.WebMercator;
                
                // 创建map points
                var point1 = MapPointBuilderEx.CreateMapPoint(West, South, spatialRef);
                var point2 = MapPointBuilderEx.CreateMapPoint(East, North, spatialRef);

                // 创建envelope
                return EnvelopeBuilderEx.CreateEnvelope(point1, point2);
            }
            catch
            {
                // 如果创建失败，返回null
                return null;
            }
        }
        
        /// <summary>
        /// 从Envelope对象创建SpatialBounds
        /// </summary>
        /// <param name="envelope">Envelope对象</param>
        /// <returns>SpatialBounds对象</returns>
        public static SpatialBounds FromEnvelope(Envelope envelope)
        {
            if (envelope == null)
                return null;
                
            return new SpatialBounds
            {
                West = envelope.XMin,
                South = envelope.YMin,
                East = envelope.XMax,
                North = envelope.YMax
            };
        }
    }
}