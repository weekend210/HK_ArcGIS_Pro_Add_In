using System;
using System.Globalization;
using System.Windows.Data;

namespace HK_AREA_SEARCH.Converters
{
    /// <summary>
    /// 验证权重和的转换器
    /// </summary>
    public class WeightSumValidationConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double sum)
            {
                return Math.Abs(sum - 1.0) < 0.001 ? "Valid" : "Invalid";
            }
            return "Invalid";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}