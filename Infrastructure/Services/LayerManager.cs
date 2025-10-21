using System;
using System.IO;
using System.Threading.Tasks;
using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;

namespace HK_AREA_SEARCH.Infrastructure.Services
{
    /// <summary>
    /// 图层管理器
    /// </summary>
    public class LayerManager
    {
        /// <summary>
        /// 添加图层到地图
        /// </summary>
        /// <param name="dataPath">数据路径</param>
        /// <returns>图层对象</returns>
        public async Task<Layer> AddLayerToMap(string dataPath)
        {
            return await QueuedTask.Run(() =>
            {
                try
                {
                    var map = MapView.Active.Map;
                    if (map == null)
                        throw new Exception("没有活动的地图");

                    Uri dataUri = new Uri(dataPath);
                    Layer layer = LayerFactory.Instance.CreateLayer(dataUri, map);

                    return layer;
                }
                catch (Exception ex)
                {
                    throw new Exception($"添加图层失败: {ex.Message}", ex);
                }
            });
        }

        /// <summary>
        /// 移除图层
        /// </summary>
        /// <param name="layer">图层对象</param>
        /// <returns>异步任务</returns>
        public async Task RemoveLayer(Layer layer)
        {
            await QueuedTask.Run(() =>
            {
                try
                {
                    var map = MapView.Active.Map;
                    if (map == null)
                        throw new Exception("没有活动的地图");

                    if (map.FindLayers(layer.Name).Count > 0)
                    {
                        map.RemoveLayer(layer);
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception($"移除图层失败: {ex.Message}", ex);
                }
            });
        }

        /// <summary>
        /// 获取活动地图
        /// </summary>
        /// <returns>地图对象</returns>
        public Map GetActiveMap()
        {
            return MapView.Active?.Map;
        }

        /// <summary>
        /// 刷新图层
        /// </summary>
        /// <param name="layer">图层对象</param>
        /// <returns>异步任务</returns>
        public async Task RefreshLayer(Layer layer)
        {
            await QueuedTask.Run(() =>
            {
                // 图层数据变化时会自动刷新，这里可以添加额外的刷新逻辑
                // Invalidate is not available in ArcGIS Pro SDK, so we'll just return
                // The layer will refresh automatically when data changes
            });
        }
    }
}