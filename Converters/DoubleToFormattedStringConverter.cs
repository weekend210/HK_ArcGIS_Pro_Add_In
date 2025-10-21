using System;
using System.Globalization;
using System.Windows.Data;

namespace HK_AREA_SEARCH.Converters
{
    /// <summary>
    /// 将double值转换为格式化字符串的转换器
    /// </summary>
    public class DoubleToFormattedStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double doubleValue)
            {
                if (parameter is string format)
                {
                    return doubleValue.ToString(format);
                }
                return doubleValue.ToString("F2"); // 默认保留两位小数
            }
            return value?.ToString() ?? string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string stringValue)
            {
                if (double.TryParse(stringValue, out double result))
                {
                    return result;
                }
            }
            return 0.0;
        }
    }
}