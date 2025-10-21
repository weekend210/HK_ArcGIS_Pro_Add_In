using System;
using System.Collections.ObjectModel;
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

        public ICommand AddClassCommand { get; private set; }
        public ICommand RemoveClassCommand { get; private set; }
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
        }

        private void InitializeCommands()
        {
            AddClassCommand = new RelayCommand(AddClass, (object parameter) => true);
            RemoveClassCommand = new RelayCommand(RemoveClass, (object parameter) => ClassItems.Count > 1);
            OKCommand = new RelayCommand(OK, (object parameter) => ValidateClassItems());
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

        private void AddClass(object parameter)
        {
            ClassItems.Add(new IntervalClassItem
            {
                StartValue = 0,
                EndValue = 1000,
                ClassValue = 0
            });
        }

        private void RemoveClass(object parameter)
        {
            if (parameter is IntervalClassItem item && ClassItems.Contains(item))
            {
                ClassItems.Remove(item);
            }
        }

        private bool ValidateClassItems()
        {
            // 验证分类项的有效性
            foreach (var item in ClassItems)
            {
                if (item.StartValue >= item.EndValue)
                    return false;
            }
            return ClassItems.Count > 0;
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