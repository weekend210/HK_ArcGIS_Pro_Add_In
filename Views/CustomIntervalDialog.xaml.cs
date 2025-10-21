using System;
using System.Windows;
using HK_AREA_SEARCH.ViewModels;
using HK_AREA_SEARCH.Models;

namespace HK_AREA_SEARCH.Views
{
    /// <summary>
    /// 自定义间隔对话框
    /// </summary>
    public partial class CustomIntervalDialog : Window
    {
        public CustomIntervalDialog(POIDataItem poiItem)
        {
            InitializeComponent();
            
            // 创建 ViewModel 并传递 POI 项信息
            var viewModel = new CustomIntervalDialogViewModel(poiItem);
            this.DataContext = viewModel;
        }

        // 获取对话框结果的属性
        public bool? ShowDialogResult => this.DialogResult;
    }
}