using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace HK_AREA_SEARCH.Models
{
    /// <summary>
    /// POI数据项模型（支持动态空行）
    /// </summary>
    public class POIDataItem : INotifyPropertyChanged
    {
        private string _dataPath;
        private string _dataName;
        private int? _distance;
        private double? _weight;
        private bool _weightHasBeenSetByUser = false; // Track if user has set the weight
        private bool _customInterval;
        private bool _isRasterData;

        /// <summary>
        /// 输入数据路径
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

                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        DataName = System.IO.Path.GetFileNameWithoutExtension(value);

                        // 判断是否为栅格数据
                        var ext = System.IO.Path.GetExtension(value)?.ToLower();
                        IsRasterData = ext == ".tif" || ext == ".tiff" || ext == ".img";
                    }
                    else
                    {
                        DataName = string.Empty;
                        IsRasterData = false;
                    }
                }
            }
        }

        /// <summary>
        /// 数据名称
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
        /// 距离（米），正数或负数
        /// </summary>
        public int? Distance
        {
            get => _distance;
            set
            {
                // 如果是栅格数据，强制为空
                if (IsRasterData)
                {
                    _distance = null;
                }
                else
                {
                    _distance = value;
                }
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// 权重（小于1.0的浮点数，保留两位小数）
        /// </summary>
        public double? Weight
        {
            get => _weight;
            set
            {
                if (value.HasValue)
                {
                    _weight = Math.Round(value.Value, 2);
                    _weightHasBeenSetByUser = true; // Mark that user has set the value
                }
                else
                {
                    _weight = value;
                    _weightHasBeenSetByUser = false; // If set to null, reset the flag
                }
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// 权重是否已被用户设置（用于验证）
        /// </summary>
        public bool WeightHasBeenSetByUser => _weightHasBeenSetByUser;

        /// <summary>
        /// 是否自定义距离间隔
        /// </summary>
        public bool CustomInterval
        {
            get => _customInterval;
            set
            {
                if (_customInterval != value)
                {
                    _customInterval = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 是否为栅格数据
        /// </summary>
        public bool IsRasterData
        {
            get => _isRasterData;
            private set
            {
                if (_isRasterData != value)
                {
                    _isRasterData = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsDistanceEnabled));

                    // 如果是栅格数据，清空距离
                    if (_isRasterData)
                    {
                        Distance = null;
                    }
                }
            }
        }

        /// <summary>
        /// 距离字段是否可编辑
        /// </summary>
        public bool IsDistanceEnabled => !IsRasterData;

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