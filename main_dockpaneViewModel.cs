using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Input;
using Microsoft.Win32;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using HK_AREA_SEARCH.Models;
using HK_AREA_SEARCH.Divide;
using HK_AREA_SEARCH.Distance;
using HK_AREA_SEARCH.Rating;
using HK_AREA_SEARCH.Infrastructure.Services;
using HK_AREA_SEARCH.Infrastructure.Helpers;
using HK_AREA_SEARCH.Common;

namespace HK_AREA_SEARCH
{
    internal class main_dockpaneViewModel : DockPane
    {
        private const string _dockPaneID = "HK_AREA_SEARCH_main_dockpane";

        #region 属性

        private string _heading = "香港选址工具";
        public string Heading
        {
            get { return _heading; }
            set { SetProperty(ref _heading, value, () => Heading); }
        }

        /// <summary>
        /// 约束条件数据集合
        /// </summary>
        private ObservableCollection<ConstraintDataItem> _constraintItems;
        public ObservableCollection<ConstraintDataItem> ConstraintItems
        {
            get { return _constraintItems; }
            set { SetProperty(ref _constraintItems, value, () => ConstraintItems); }
        }

        /// <summary>
        /// 分析区域路径
        /// </summary>
        private string _analysisAreaPath;
        public string AnalysisAreaPath
        {
            get { return _analysisAreaPath; }
            set { SetProperty(ref _analysisAreaPath, value, () => AnalysisAreaPath); }
        }

        /// <summary>
        /// POI数据集合
        /// </summary>
        private ObservableCollection<POIDataItem> _poiItems;
        public ObservableCollection<POIDataItem> POIItems
        {
            get { return _poiItems; }
            set { SetProperty(ref _poiItems, value, () => POIItems); }
        }

        /// <summary>
        /// 输出路径
        /// </summary>
        private string _outputPath;
        public string OutputPath
        {
            get { return _outputPath; }
            set { SetProperty(ref _outputPath, value, () => OutputPath); }
        }

        /// <summary>
        /// 权重和验证消息
        /// </summary>
        private string _weightSumValidationMessage;
        public string WeightSumValidationMessage
        {
            get { return _weightSumValidationMessage; }
            set { SetProperty(ref _weightSumValidationMessage, value, () => WeightSumValidationMessage); }
        }

        /// <summary>
        /// 权重和是否有效
        /// </summary>
        private bool _isWeightSumValid;
        public bool IsWeightSumValid
        {
            get { return _isWeightSumValid; }
            set { SetProperty(ref _isWeightSumValid, value, () => IsWeightSumValid); }
        }



        #endregion

        #region 命令

        /// <summary>
        /// 浏览约束条件数据命令
        /// </summary>
        public ICommand BrowseConstraintCommand { get; private set; }

        /// <summary>
        /// 浏览分析区域命令
        /// </summary>
        public ICommand BrowseAnalysisAreaCommand { get; private set; }

        /// <summary>
        /// 浏览POI数据命令
        /// </summary>
        public ICommand BrowsePOICommand { get; private set; }

        /// <summary>
        /// 浏览输出路径命令
        /// </summary>
        public ICommand BrowseOutputCommand { get; private set; }

        /// <summary>
        /// 运行分析命令
        /// </summary>
        public ICommand RunAnalysisCommand { get; private set; }

        #endregion

        #region 构造函数

        protected main_dockpaneViewModel()
        {
            InitializeCollections();
            InitializeCommands();
        }

        /// <summary>
        /// 初始化集合
        /// </summary>
        private void InitializeCollections()
        {
            // 初始化约束条件集合（只添加1个空白行）
            ConstraintItems = new ObservableCollection<ConstraintDataItem>();
            AddEmptyConstraintRow();

            // 初始化POI集合
            POIItems = new ObservableCollection<POIDataItem>();
            AddEmptyPOIRow();
        }

        private void InitializeCommands()
        {
            BrowseConstraintCommand = new RelayCommand(BrowseConstraintData, (object parameter) => true);
            BrowseAnalysisAreaCommand = new RelayCommand(BrowseAnalysisArea, (object parameter) => true);
            BrowsePOICommand = new RelayCommand(BrowsePOIData, (object parameter) => true);
            BrowseOutputCommand = new RelayCommand(BrowseOutputPath, (object parameter) => true);
            RunAnalysisCommand = new RelayCommand(async (param) => await RunAnalysisAsync(param), CanRunAnalysis);
        }

        #endregion

        #region 动态空行管理

        /// <summary>
        /// 添加空白约束条件行，并订阅属性变化事件
        /// </summary>
        private void AddEmptyConstraintRow()
        {
            var newItem = new ConstraintDataItem();

            // 订阅属性变化事件
            newItem.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(ConstraintDataItem.DataPath))
                {
                    CheckAndAddEmptyConstraintRow();
                }
            };

            ConstraintItems.Add(newItem);
        }

        /// <summary>
        /// 添加空白POI行，并订阅属性变化事件
        /// </summary>
        private void AddEmptyPOIRow()
        {
            var newItem = new POIDataItem();

            // 订阅属性变化事件
            newItem.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(POIDataItem.DataPath))
                {
                    CheckAndAddEmptyPOIRow();
                }

                // 权重变化时验证总和
                if (e.PropertyName == nameof(POIDataItem.Weight))
                {
                    ValidateWeightSum();
                }
            };

            POIItems.Add(newItem);
        }

        /// <summary>
        /// 检查并添加空白约束条件行
        /// </summary>
        private void CheckAndAddEmptyConstraintRow()
        {
            // 检查最后一行是否为空白行
            var lastItem = ConstraintItems.LastOrDefault();

            if (lastItem != null && !lastItem.IsEmpty)
            {
                // 最后一行已被填充，添加新空白行
                AddEmptyConstraintRow();
            }
        }

        /// <summary>
        /// 检查并添加空白POI行
        /// </summary>
        private void CheckAndAddEmptyPOIRow()
        {
            // 检查最后一行是否为空白行
            var lastItem = POIItems.LastOrDefault();

            if (lastItem != null && !lastItem.IsEmpty)
            {
                // 最后一行已被填充，添加新空白行
                AddEmptyPOIRow();
            }
        }

        #endregion

        #region 方法

        /// <summary>
        /// 验证权重和
        /// </summary>
        private void ValidateWeightSum()
        {
            var nonEmptyItems = POIItems.Where(p => !p.IsEmpty && p.Weight.HasValue).ToList();

            if (nonEmptyItems.Count == 0)
            {
                IsWeightSumValid = false;
                WeightSumValidationMessage = "";
                return;
            }

            double sum = nonEmptyItems.Sum(p => p.Weight.Value);

            if (Math.Abs(sum - 1.0) > 0.001) // 允许0.001的误差
            {
                IsWeightSumValid = true; // 显示消息
                WeightSumValidationMessage = $"警告: 权重之和为 {sum:F2}，应为 1.0";
            }
            else
            {
                IsWeightSumValid = false; // 隐藏消息
                WeightSumValidationMessage = "";
            }
        }

        /// <summary>
        /// 浏览约束条件数据
        /// </summary>
        private void BrowseConstraintData(object parameter)
        {
            var item = parameter as ConstraintDataItem;
            if (item == null) return;

            var dialog = new OpenFileDialog
            {
                Filter = "Shapefile (*.shp)|*.shp|GeoDatabase Feature Class|*.gdb|All Files (*.*)|*.*",
                Title = "选择约束条件数据"
            };

            if (dialog.ShowDialog() == true)
            {
                item.DataPath = dialog.FileName;
            }
        }

        /// <summary>
        /// 浏览分析区域
        /// </summary>
        private void BrowseAnalysisArea(object parameter)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Shapefile (*.shp)|*.shp|GeoDatabase Feature Class|*.gdb|All Files (*.*)|*.*",
                Title = "选择分析区域"
            };

            if (dialog.ShowDialog() == true)
            {
                AnalysisAreaPath = dialog.FileName;
            }
        }

        /// <summary>
        /// 浏览POI数据
        /// </summary>
        private void BrowsePOIData(object parameter)
        {
            var item = parameter as POIDataItem;
            if (item == null) return;

            var dialog = new OpenFileDialog
            {
                Filter = "所有支持格式|*.shp;*.tif;*.tiff;*.img|Shapefile (*.shp)|*.shp|Raster (*.tif;*.tiff;*.img)|*.tif;*.tiff;*.img|All Files (*.*)|*.*",
                Title = "选择POI数据或栅格"
            };

            if (dialog.ShowDialog() == true)
            {
                item.DataPath = dialog.FileName;
            }
        }

        /// <summary>
        /// 浏览输出路径
        /// </summary>
        private void BrowseOutputPath(object parameter)
        {
            var dialog = new SaveFileDialog
            {
                Filter = "Shapefile (*.shp)|*.shp",
                Title = "选择输出路径",
                FileName = "Result_Rating_Suitable_Area.shp"
            };

            if (dialog.ShowDialog() == true)
            {
                OutputPath = dialog.FileName;
            }
        }

        /// <summary>
        /// 检查是否可以运行分析
        /// </summary>
        private bool CanRunAnalysis(object parameter)
        {
            // Basic validation first
            if (string.IsNullOrWhiteSpace(AnalysisAreaPath) ||
                !POIItems.Any(p => !p.IsEmpty) ||
                string.IsNullOrWhiteSpace(OutputPath))
            {
                return false;
            }

            // Validate detailed requirements for each POI item
            foreach (var item in POIItems.Where(p => !p.IsEmpty))
            {
                // For vector data (non-raster), distance is required
                if (!item.IsRasterData && !item.Distance.HasValue)
                {
                    return false;
                }

                // Weight is always required and must be set by user
                if (!item.Weight.HasValue || !item.WeightHasBeenSetByUser)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 运行分析（异步版本）
        /// </summary>
        private async System.Threading.Tasks.Task RunAnalysisAsync(object parameter)
        {
            try
            {
                // 1. 数据验证
                var validationResult = ValidationHelper.ValidateInputs(
                    AnalysisAreaPath,
                    ConstraintItems.Where(c => !c.IsEmpty).ToList(),
                    POIItems.Where(p => !p.IsEmpty).ToList(),
                    OutputPath);

                if (!validationResult.IsValid)
                {
                    MessageBox.Show(validationResult.ErrorMessage, "验证失败");
                    return;
                }

                // 2. 初始化临时文件管理器
                var tempFileManager = new TempFileManager();

                // 3. 执行模块1: 划分可建设土地
                var divideService = new DivideService(tempFileManager);
                var suitableAreaPath = await divideService.ExecuteAsync(
                    AnalysisAreaPath,
                    ConstraintItems.Where(c => !c.IsEmpty).Select(c => c.DataPath).ToList(),
                    tempFileManager.CreateTempFile(Constants.SUITABLE_AREA_FILENAME)
                );
                
                // 4. 执行模块2: 距离计算
                var distanceService = new DistanceService(tempFileManager);
                
                var processedRasters = await distanceService.ExecuteAsync(
                    POIItems.Where(p => !p.IsEmpty).ToList(),
                    AnalysisAreaPath
                );

                // 6. 执行模块3: 评分计算
                var ratingService = new RatingService(tempFileManager);
                var weights = POIItems.Where(p => !p.IsEmpty)
                    .ToDictionary(p => p.DataName, p => p.Weight.Value);
                var resultPath = await ratingService.ExecuteAsync(
                    processedRasters,
                    weights,
                    suitableAreaPath,
                    OutputPath
                );
/*
                // 7. 清理临时文件
                tempFileManager.CleanupAll();

                // 8. 加载结果到地图
                var layerManager = new LayerManager();
                var layer = await layerManager.AddLayerToMap(resultPath);

                // 9. 应用符号系统
                var symbologyManager = new SymbologyManager();
                await symbologyManager.ApplyGraduatedColors(layer, Constants.RATING_FIELD);
*/

                // 10. 显示成功消息
                MessageBox.Show("选址分析完成!", "成功");
            }
            catch (Exception ex)
            {
                LogService.LogError($"分析失败: {ex.Message}");
                
                MessageBox.Show($"分析失败: {ex.Message}", "错误");
                
            }
        }

        #endregion

        #region DockPane方法

        internal static void Show()
        {
            DockPane pane = FrameworkApplication.DockPaneManager.Find(_dockPaneID);
            if (pane == null)
                return;

            pane.Activate();
        }

        #endregion
        
    }

    /// <summary>
    /// 按钮实现以显示DockPane
    /// </summary>
    internal class main_dockpane_ShowButton : Button
    {
        protected override void OnClick()
        {
            main_dockpaneViewModel.Show();
        }
    }
}
