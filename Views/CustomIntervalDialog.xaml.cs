using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using HK_AREA_SEARCH.ViewModels;

namespace HK_AREA_SEARCH.Views
{
    /// <summary>
    /// CustomIntervalDialog.xaml 的交互逻辑
    /// </summary>
    public partial class CustomIntervalDialog : Window
    {
        public CustomIntervalDialog()
        {
            InitializeComponent();
            this.DataContext = new CustomIntervalDialogViewModel();
        }

        public CustomIntervalDialog(CustomIntervalDialogViewModel viewModel)
        {
            InitializeComponent();
            this.DataContext = viewModel;
        }
    }
}