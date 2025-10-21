using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace HK_AREA_SEARCH.Models
{
    /// <summary>
    /// 约束条件数据项模型（支持动态空行）
    /// </summary>
    public class ConstraintDataItem : INotifyPropertyChanged
    {
        private string _dataPath;
        private string _dataName;

        /// <summary>
        /// 数据文件路径
        /// </summary>
        public string DataPath
        {
            get => _dataPath;
            set
            {
                if (_dataPath != value)
                {
                    _dataPath = value;
                    OnPropertyChanged();

                    // 自动提取文件名
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        DataName = System.IO.Path.GetFileNameWithoutExtension(value);
                    }
                    else
                    {
                        DataName = string.Empty;
                    }
                }
            }
        }

        /// <summary>
        /// 数据名称（显示用）
        /// </summary>
        public string DataName
        {
            get => _dataName;
            set
            {
                if (_dataName != value)
                {
                    _dataName = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 是否为空白行
        /// </summary>
        public bool IsEmpty => string.IsNullOrWhiteSpace(DataPath);

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}