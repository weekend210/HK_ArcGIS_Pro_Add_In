using System;
using System.Globalization;
using System.Windows.Data;

namespace HK_AREA_SEARCH.Converters
{
    /// <summary>
    /// 将布尔值转换为"是/否"字符串的转换器
    /// </summary>
    public class BooleanToYesNoConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? "是" : "否";
            }
            return "否";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string stringValue)
            {
                return stringValue == "是";
            }
            return false;
        }
    }
}