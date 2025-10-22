using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using ArcGIS.Desktop.Framework.Contracts;
using HK_AREA_SEARCH.Models;
using HK_AREA_SEARCH.Common;

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
        
        private POIDataItem _poiItem;

        public ICommand OKCommand { get; private set; }
        public ICommand CancelCommand { get; private set; }

        private bool _dialogResult;
        public bool DialogResult
        {
            get { return _dialogResult; }
            set { SetProperty(ref _dialogResult, value, () => DialogResult); }
        }

        public CustomIntervalDialogViewModel(POIDataItem poiItem)
        {
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
            int distance = _poiItem?.Distance != null ? Math.Abs(_poiItem.Distance.Value) : 1000;   // 使用绝对值
            
            double minValue = 0;
            double maxValue = distance;
            double interval = (maxValue - minValue) / ClassNumber;

            if (_poiItem?.Distance > 0)
            {
                // 如果距离为正，分类值从10到1（反向评分，距离越近分越高）
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
                // 如果距离为负，分类值从1到10（距离越近分越低）
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