using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace HK_AREA_SEARCH.Models
{
    /// <summary>
    /// 分类间隔项模型（用于自定义距离间隔对话框）
    /// </summary>
    public class IntervalClassItem : INotifyPropertyChanged
    {
        private double _startValue;
        private double _endValue;
        private int _classValue;

        /// <summary>
        /// 开始值
        /// </summary>
        public double StartValue
        {
            get => _startValue;
            set
            {
                if (_startValue != value)
                {
                    _startValue = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 结束值
        /// </summary>
        public double EndValue
        {
            get => _endValue;
            set
            {
                if (_endValue != value)
                {
                    _endValue = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 类别值（0-10或10-0）
        /// </summary>
        public int ClassValue
        {
            get => _classValue;
            set
            {
                if (_classValue != value)
                {
                    _classValue = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}