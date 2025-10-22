using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using ArcGIS.Desktop.Framework.Contracts;
using HK_AREA_SEARCH.Models;
using HK_AREA_SEARCH.Common;
using HK_AREA_SEARCH.Infrastructure.Services;
using HK_AREA_SEARCH.Distance;

namespace HK_AREA_SEARCH.ViewModels
{
    /// <summary>
    /// 自定义间隔对话框视图模型
    /// </summary>
    public class CustomIntervalDialogViewModel : PropertyChangedBase
    {
        public ObservableCollection<IntervalClassItem> ClassItems { get; set; }
        
        private string _title;
        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value, () => Title); }
        }
        
        private string _inputRasterPath;
        private POIDataItem _poiItem;

        public ICommand OKCommand { get; private set; }
        public ICommand CancelCommand { get; private set; }

        private bool _dialogResult;
        public bool DialogResult
        {
            get { return _dialogResult; }
            set { SetProperty(ref _dialogResult, value, () => DialogResult); }
        }

        public CustomIntervalDialogViewModel(string inputRasterPath, POIDataItem poiItem)
        {
            _inputRasterPath = inputRasterPath;
            _poiItem = poiItem;
            _title = $"自定义距离间隔 - {poiItem?.DataName ?? "未知数据"}";
            ClassItems = new ObservableCollection<IntervalClassItem>();
            // 初始化默认分类
            InitializeDefaultClasses();
            InitializeCommands();
            
            // 订阅ClassItems集合中每个项目的属性变化事件
            foreach (var item in ClassItems)
            {
                ((INotifyPropertyChanged)item).PropertyChanged += OnIntervalItemPropertyChanged;
            }
            
            // 订阅集合变化事件 if the collection could be modified in the future
            ClassItems.CollectionChanged += (sender, e) =>
            {
                if (e.NewItems != null)
                {
                    foreach (IntervalClassItem item in e.NewItems)
                    {
                        ((INotifyPropertyChanged)item).PropertyChanged += OnIntervalItemPropertyChanged;
                    }
                }
                if (e.OldItems != null)
                {
                    foreach (IntervalClassItem item in e.OldItems)
                    {
                        ((INotifyPropertyChanged)item).PropertyChanged -= OnIntervalItemPropertyChanged;
                    }
                }
            };
        }

        private void InitializeCommands()
        {
            OKCommand = new RelayCommandWithCanExecute(OK, (object parameter) => ValidateClassItems());
            CancelCommand = new RelayCommand(Cancel, (object parameter) => true);
        }

        private void InitializeDefaultClasses()
        {
            int ClassNumber = Constants.NUM_CLASSES;
            
            double minValue, maxValue;
            
            // 如果有距离值（矢量数据），则使用距离
            // 如果没有距离值（栅格数据），则使用默认值 initially
            if (_poiItem?.Distance.HasValue == true)
            {
                int distance = Math.Abs(_poiItem.Distance.Value);   // 使用绝对值
                minValue = 0;
                maxValue = distance;
            }
            else
            {
                // 对于栅格数据，暂时使用默认值
                minValue = 0;
                maxValue = 1000;
            }
            
            double interval = (maxValue - minValue) / ClassNumber;

            // 确定分类值的顺序：如果距离为正，则反向（高值对应低分类），否则正向
            bool invertValues = _poiItem?.Distance.HasValue == true ? _poiItem.Distance > 0 : true;

            // 清空现有项
            ClassItems.Clear();

            if (invertValues)
            {
                // 如果距离为正或未知（栅格数据），分类值从高到低（反向评分，值越小分越高）
                for (int i = 0; i < ClassNumber; i++)
                {
                    double fromValue = minValue + (i * interval);
                    double toValue = (i == ClassNumber - 1) ? maxValue : minValue + ((i + 1) * interval); // 确保最后一个区间包含最大值
                    int classValue = ClassNumber - i; // 反转分类值

                    ClassItems.Add(new IntervalClassItem
                    {
                        StartValue = fromValue,
                        EndValue = toValue,
                        ClassValue = classValue
                    });
                }
            }
            else
            {
                // 如果距离为负，分类值从低到高
                for (int i = 0; i < ClassNumber; i++)
                {
                    double fromValue = minValue + (i * interval);
                    double toValue = (i == ClassNumber - 1) ? maxValue : minValue + ((i + 1) * interval); // 确保最后一个区间包含最大值
                    int classValue = i + 1;

                    ClassItems.Add(new IntervalClassItem
                    {
                        StartValue = fromValue,
                        EndValue = toValue,
                        ClassValue = classValue
                    });
                }
            }
            
            // 如果没有距离值（栅格数据），在后台获取实际的栅格 min/max 值并更新分类
            if (!_poiItem?.Distance.HasValue == true && !string.IsNullOrEmpty(_inputRasterPath))
            {
                _ = UpdateClassItemsWithRasterValues();  // Fire and forget
            }
        }
        
        private async Task UpdateClassItemsWithRasterValues()
        {
            try
            {
                // Create a temporary TempFileManager for this operation
                var tempFileManager = new TempFileManager();
                var reclassifier = new RasterReclassifier(tempFileManager);
                
                var (rasterMin, rasterMax) = await reclassifier.GetRasterMinMaxAsync(_inputRasterPath);
                await Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    UpdateClassItemsWithValues(rasterMin, rasterMax);
                }));
            }
            catch (Exception ex)
            {
                // In case of error, keep the default values, but perhaps log the error
                System.Diagnostics.Debug.WriteLine($"Error getting raster min/max: {ex.Message}");
            }
        }
        
        private void UpdateClassItemsWithValues(double minValue, double maxValue)
        {
            int ClassNumber = Constants.NUM_CLASSES;
            double interval = (maxValue - minValue) / ClassNumber;

            // For raster data without distance, we'll use the standard approach 
            // (lower raster values get lower class values)
            // Determine whether to invert values based on context - for now, defaulting to non-inverted for raster
            bool invertValues = false; // default for raster data

            // Clear existing items
            ClassItems.Clear();

            if (invertValues)
            {
                // 反向：值越小，分类越高
                for (int i = 0; i < ClassNumber; i++)
                {
                    double fromValue = minValue + (i * interval);
                    double toValue = (i == ClassNumber - 1) ? maxValue : minValue + ((i + 1) * interval);
                    int classValue = ClassNumber - i;

                    ClassItems.Add(new IntervalClassItem
                    {
                        StartValue = fromValue,
                        EndValue = toValue,
                        ClassValue = classValue
                    });
                }
            }
            else
            {
                // 正向：值越小，分类越低
                for (int i = 0; i < ClassNumber; i++)
                {
                    double fromValue = minValue + (i * interval);
                    double toValue = (i == ClassNumber - 1) ? maxValue : minValue + ((i + 1) * interval);
                    int classValue = i + 1;

                    ClassItems.Add(new IntervalClassItem
                    {
                        StartValue = fromValue,
                        EndValue = toValue,
                        ClassValue = classValue
                    });
                }
            }
        }



        private bool ValidateClassItems()
        {
            // 首先重置所有错误标志
            foreach (var item in ClassItems)
            {
                item.HasStartValueError = false;
                item.HasEndValueError = false;
            }

            // 验证区间连续性和有效性
            for (int i = 0; i < ClassItems.Count; i++)
            {
                var currentItem = ClassItems[i];
                
                // 检查每个区间的开始值必须小于结束值
                if (currentItem.StartValue >= currentItem.EndValue)
                {
                    currentItem.HasStartValueError = true;
                    currentItem.HasEndValueError = true;
                    continue; // 如果区间本身无效，无需检查连续性
                }

                // 从第二个区间开始，检查连续性：当前区间的开始值应该等于前一个区间的结束值
                if (i > 0)
                {
                    var previousItem = ClassItems[i - 1];
                    if (Math.Abs(currentItem.StartValue - previousItem.EndValue) > 0.0001) // 使用小的容差值
                    {
                        currentItem.HasStartValueError = true;
                        previousItem.HasEndValueError = true;
                    }
                }
            }

            // 检查是否有任何错误
            bool hasErrors = ClassItems.Any(item => item.HasStartValueError || item.HasEndValueError);
            
            return !hasErrors && ClassItems.Count > 0;
        }

        private void OK(object parameter)
        {
            DialogResult = true;
            CloseDialog();
        }

        private void Cancel(object parameter)
        {
            DialogResult = false;
            CloseDialog();
        }

        private void OnIntervalItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // 当区间值改变时，重新验证整个列表
            if (e.PropertyName == nameof(IntervalClassItem.StartValue) || 
                e.PropertyName == nameof(IntervalClassItem.EndValue))
            {
                // 更新命令的可执行状态
                if (OKCommand is RelayCommandWithCanExecute okCommand)
                {
                    okCommand.RaiseCanExecuteChanged();
                }
            }
        }

        private void CloseDialog()
        {
            var window = Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.DataContext == this);
            if (window != null)
            {
                window.DialogResult = _dialogResult;
                window.Close();
            }
        }
    }
}