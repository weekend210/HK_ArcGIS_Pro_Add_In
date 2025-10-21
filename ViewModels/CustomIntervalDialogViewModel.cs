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

        public CustomIntervalDialogViewModel()
        {
            ClassItems = new ObservableCollection<IntervalClassItem>();
            InitializeCommands();
            // 初始化默认分类
            InitializeDefaultClasses();
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
            // 添加默认分类
            for (int i = 0; i < ClassNumber; i++)
            {
                ClassItems.Add(new IntervalClassItem
                {
                    StartValue = i * 100,  // 示例：距离间隔100米
                    EndValue = (i + 1) * 100,
                    ClassValue = 10 - i     // 反向评分，距离越近分越高
                });
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
                window.DialogResult = DialogResult;
                window.Close();
            }
        }
    }
}