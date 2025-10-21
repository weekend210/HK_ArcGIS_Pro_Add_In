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

        /// <summary>
        /// 检查_startValue、_endValue、_classValue三值是否为空或默认值
        /// </summary>
        /// <returns>如果三个值都为默认值（0）则返回true，否则返回false</returns>
        public bool IsEmpty()
        {
            // 检查_startValue、_endValue、_classValue是否都为默认值
            return _startValue == 0.0 && _endValue == 0.0 && _classValue == 0;
        }
    }
}