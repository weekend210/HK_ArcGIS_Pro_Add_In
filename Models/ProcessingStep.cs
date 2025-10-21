using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace HK_AREA_SEARCH.Models
{
    /// <summary>
    /// 处理步骤模型
    /// </summary>
    public class ProcessingStep : INotifyPropertyChanged
    {
        private StepStatus _status;
        private int _progressPercentage;
        private string _message;

        /// <summary>
        /// 步骤名称
        /// </summary>
        public string StepName { get; set; }

        /// <summary>
        /// 步骤状态
        /// </summary>
        public StepStatus Status
        {
            get => _status;
            set
            {
                if (_status != value)
                {
                    _status = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 进度百分比
        /// </summary>
        public int ProgressPercentage
        {
            get => _progressPercentage;
            set
            {
                if (_progressPercentage != value)
                {
                    _progressPercentage = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 消息
        /// </summary>
        public string Message
        {
            get => _message;
            set
            {
                if (_message != value)
                {
                    _message = value;
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

    public enum StepStatus
    {
        Pending,
        Running,
        Completed,
        Failed
    }
}